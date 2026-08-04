using System;
using System.Linq;
using System.Xml.Linq;
using NFSe.DANFSe.v2.Models;

namespace NFSe.DANFSe.v2.Parser
{
    public static class DanfseXmlParser
    {
        public static DanfseModel Parse(string xmlText)
        {
            if (string.IsNullOrEmpty(xmlText))
                throw new ArgumentException("O XML da nota não pode estar vazio.", nameof(xmlText));

            var doc = XDocument.Parse(xmlText);
            var root = doc.Root;

            if (root == null)
                throw new Exception("XML inválido: Elemento raiz não encontrado.");

            XNamespace rootNs = root.Name.Namespace;

            // Se o XML recebido for um evento, criamos um modelo básico apenas com as informações do evento
            if (root.Name.LocalName.Equals("evento", StringComparison.OrdinalIgnoreCase))
            {
                var model = new DanfseModel();
                ApplyEventInternal(model, root, rootNs);
                return model;
            }

            // O XML de nota pode ter a raiz como <NFSe> ou estar envelopada. Buscamos o nó <infNFSe>
            var infNFSe = root.Name.LocalName.Equals("infNFSe", StringComparison.OrdinalIgnoreCase) 
                ? root 
                : root.Element(rootNs + "infNFSe") ?? root.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("infNFSe", StringComparison.OrdinalIgnoreCase));

            if (infNFSe == null)
                throw new Exception("XML inválido: Elemento 'infNFSe' não encontrado.");

            XNamespace ns = infNFSe.Name.Namespace;

            string xmlVersao = GetAttributeValue(root, "versao");
            if (string.IsNullOrEmpty(xmlVersao))
            {
                xmlVersao = GetAttributeValue(infNFSe, "versao");
            }
            if (string.IsNullOrEmpty(xmlVersao))
            {
                xmlVersao = "1.01";
            }

            var modelResult = new DanfseModel
            {
                Id = GetAttributeValue(infNFSe, "Id"),
                NNFSe = GetElementValue(infNFSe, ns + "nNFSe"),
                CStat = GetElementValue(infNFSe, ns + "cStat"),
                DhProc = GetElementValue(infNFSe, ns + "dhProc"),
                CLocIncid = GetElementValue(infNFSe, ns + "cLocIncid"),
                XLocIncid = GetElementValue(infNFSe, ns + "xLocIncid"),
                XTribNac = GetElementValue(infNFSe, ns + "xTribNac"),
                XNBS = GetElementValue(infNFSe, ns + "xNBS"),
                AmbGer = GetElementValue(infNFSe, ns + "ambGer"),
                Versao = xmlVersao
            };

            // Emitente
            var emit = infNFSe.Element(ns + "emit");
            if (emit != null)
            {
                modelResult.Emitente = ParseParty(emit, ns);
            }

            // Valores Gerais
            var valores = infNFSe.Element(ns + "valores");
            if (valores != null)
            {
                modelResult.Valores = new ValoresData
                {
                    VLiq = GetElementValue(valores, ns + "vLiq"),
                    VTotalRet = GetElementValue(valores, ns + "vTotalRet"),
                    TpBM = GetElementValue(valores, ns + "tpBM"),
                    VCalcBM = GetElementValue(valores, ns + "vCalcBM"),
                    VCalcDR = GetElementValue(valores, ns + "vCalcDR"),
                    VBC = GetElementValue(valores, ns + "vBC"),
                    PAliqAplic = GetElementValue(valores, ns + "pAliqAplic"),
                    VIssqn = GetElementValue(valores, ns + "vISSQN")
                };
            }

            // IBS / CBS (Reforma Tributária)
            var ibsCbs = infNFSe.Element(ns + "IBSCBS");
            if (ibsCbs != null)
            {
                modelResult.IbsCbs = ParseIbsCbs(ibsCbs, ns);
            }

            // DPS (dados originais de emissão)
            var dps = infNFSe.Element(ns + "DPS");
            if (dps != null)
            {
                var infDPS = dps.Element(ns + "infDPS");
                if (infDPS != null)
                {
                    XNamespace dpsNs = infDPS.Name.Namespace;
                    modelResult.TpAmb = GetElementValue(infDPS, dpsNs + "tpAmb");
                    modelResult.Dps = new DpsData
                    {
                        Id = GetAttributeValue(infDPS, "Id"),
                        DhEmi = GetElementValue(infDPS, dpsNs + "dhEmi"),
                        Serie = GetElementValue(infDPS, dpsNs + "serie"),
                        NDPS = GetElementValue(infDPS, dpsNs + "nDPS"),
                        DCompet = GetElementValue(infDPS, dpsNs + "dCompet"),
                        TpEmit = GetElementValue(infDPS, dpsNs + "tpEmit")
                    };

                    // Prestador
                    var prest = infDPS.Element(dpsNs + "prest");
                    if (prest != null)
                    {
                        modelResult.Prestador = ParsePrestador(prest, dpsNs);
                    }

                    // Tomador
                    var toma = infDPS.Element(dpsNs + "toma");
                    if (toma != null)
                    {
                        modelResult.Tomador = ParseParty(toma, dpsNs);
                    }

                    // Destinatário
                    var dest = infDPS.Element(dpsNs + "dest");
                    if (dest != null)
                    {
                        modelResult.Destinatario = ParseParty(dest, dpsNs);
                    }

                    // Intermediário
                    var interm = infDPS.Element(dpsNs + "interm");
                    if (interm != null)
                    {
                        modelResult.Intermediario = ParseParty(interm, dpsNs);
                    }

                    // Serviço Prestado
                    var serv = infDPS.Element(dpsNs + "serv");
                    if (serv != null)
                    {
                        modelResult.Servico = ParseServico(serv, infDPS, dpsNs);
                    }

                    // Complementa IBS/CBS com dados da DPS (ex: cIndOp, CST, cClassTrib) se existirem
                    var ibsCbsDps = infDPS.Element(dpsNs + "IBSCBS");
                    if (ibsCbsDps != null)
                    {
                        ParseIbsCbsElement(modelResult.IbsCbs, ibsCbsDps, dpsNs);
                    }
                }
            }

            return modelResult;
        }

        public static void ApplyEvent(DanfseModel model, string eventXmlText)
        {
            if (string.IsNullOrEmpty(eventXmlText)) return;

            var doc = XDocument.Parse(eventXmlText);
            var root = doc.Root;
            if (root == null) return;

            XNamespace ns = root.Name.Namespace;
            ApplyEventInternal(model, root, ns);
        }

        private static void ApplyEventInternal(DanfseModel model, XElement root, XNamespace rootNs)
        {
            var infEvento = root.Name.LocalName.Equals("infEvento", StringComparison.OrdinalIgnoreCase)
                ? root
                : root.Element(rootNs + "infEvento") ?? root.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("infEvento", StringComparison.OrdinalIgnoreCase));

            if (infEvento == null) return;

            var pedRegEvento = infEvento.Element(rootNs + "pedRegEvento");
            if (pedRegEvento == null) return;

            var infPedReg = pedRegEvento.Element(rootNs + "infPedReg");
            if (infPedReg == null) return;

            var chNFSe = GetElementValue(infPedReg, rootNs + "chNFSe");
            if (!string.IsNullOrEmpty(chNFSe))
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = chNFSe;
                }
            }

            // Verifica o tipo de evento (Ex: e101101 = Cancelamento, e101102 = Substituição)
            var cancel = infPedReg.Element(rootNs + NfseSchemaValues.EventoCancelamento);
            if (cancel != null)
            {
                model.IsCancelled = true;
            }

            var sub = infPedReg.Element(rootNs + NfseSchemaValues.EventoSubstituicao1) ?? infPedReg.Element(rootNs + NfseSchemaValues.EventoSubstituicao2);
            if (sub != null)
            {
                model.IsSubstituted = true;
            }

            // Fallback para descrição em texto
            var desc = infPedReg.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("xDesc", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
            if (desc.Contains("Cancelamento", StringComparison.OrdinalIgnoreCase))
            {
                model.IsCancelled = true;
            }
            else if (desc.Contains("Substitu", StringComparison.OrdinalIgnoreCase))
            {
                model.IsSubstituted = true;
            }
        }

        private static Party ParseParty(XElement elem, XNamespace ns)
        {
            var p = new Party
            {
                Documento = GetElementValue(elem, ns + "CNPJ"),
                IM = GetElementValue(elem, ns + "IM"),
                Nome = GetElementValue(elem, ns + "xNome"),
                Fone = GetElementValue(elem, ns + "fone"),
                Email = GetElementValue(elem, ns + "email"),
                Identificado = true
            };

            if (string.IsNullOrEmpty(p.Documento))
            {
                p.Documento = GetElementValue(elem, ns + "CPF");
            }

            // Estrutura 1 (emit/toma no infNFSe): <enderNac> é filho direto
            var ender = elem.Element(ns + "enderNac");
            if (ender != null)
            {
                p.Endereco = ParseEndereco(ender, ns);
            }
            else
            {
                // Estrutura 2 (toma/dest/interm no DPS): <end> contém <endNac> + campos de logradouro
                var endElem = elem.Element(ns + "end");
                if (endElem != null)
                {
                    var endNac = endElem.Element(ns + "endNac");
                    p.Endereco = new Endereco
                    {
                        // Campos do <endNac>: cMun e CEP (TCEnderNac no schema)
                        // xMun e UF não existem em TCEnderNac; xMun é derivado via IbgeResolver no renderer
                        CMun = endNac != null ? GetElementValue(endNac, ns + "cMun") : string.Empty,
                        XMun = string.Empty,
                        UF = string.Empty,
                        CEP = endNac != null ? GetElementValue(endNac, ns + "CEP") : string.Empty,
                        // Campos de logradouro que ficam em <end> (fora do <endNac>)
                        Logradouro = GetElementValue(endElem, ns + "xLgr"),
                        Numero = GetElementValue(endElem, ns + "nro"),
                        Complemento = GetElementValue(endElem, ns + "xCpl"),
                        Bairro = GetElementValue(endElem, ns + "xBairro")
                    };
                }
            }

            return p;
        }

        private static Endereco ParseEndereco(XElement elem, XNamespace ns)
        {
            return new Endereco
            {
                Logradouro = GetElementValue(elem, ns + "xLgr"),
                Numero = GetElementValue(elem, ns + "nro"),
                Complemento = GetElementValue(elem, ns + "xCpl"),
                Bairro = GetElementValue(elem, ns + "xBairro"),
                CMun = GetElementValue(elem, ns + "cMun"),
                // xMun não existe em TCEnderecoEmitente no schema; será derivado via IbgeResolver no renderer
                XMun = string.Empty,
                UF = GetElementValue(elem, ns + "UF"),
                CEP = GetElementValue(elem, ns + "CEP")
            };
        }

        private static PrestadorData ParsePrestador(XElement elem, XNamespace ns)
        {
            var regTrib = elem.Element(ns + "regTrib");
            return new PrestadorData
            {
                IM = GetElementValue(elem, ns + "IM"),
                Fone = GetElementValue(elem, ns + "fone"),
                Email = GetElementValue(elem, ns + "email"),
                OpSimpNac = regTrib != null ? GetElementValue(regTrib, ns + "opSimpNac") : string.Empty,
                RegApTribSN = regTrib != null ? GetElementValue(regTrib, ns + "regApTribSN") : string.Empty,
                RegEspTrib = regTrib != null ? GetElementValue(regTrib, ns + "regEspTrib") : string.Empty
            };
        }

        private static ServicoData ParseServico(XElement elem, XElement infDPS, XNamespace ns)
        {
            var locPrest = elem.Element(ns + "locPrest");
            var obra = elem.Element(ns + "obra");
            var cServ = elem.Element(ns + "cServ");
            var infoCompl = elem.Element(ns + "infoCompl");

            // Elemento 'valores' irmão de 'serv' no infDPS, que contém 'trib'
            var valoresDps = infDPS.Element(ns + "valores");
            XElement? trib = null;
            if (valoresDps != null)
            {
                trib = valoresDps.Element(ns + "trib");
            }

            var data = new ServicoData
            {
                CLocPrestacao = locPrest != null ? GetElementValue(locPrest, ns + "cLocPrestacao") : string.Empty,
                // xMun e CEP não existem em TCLocPrest no schema; o nome do município é derivado via IbgeResolver
                XMunPrestacao = string.Empty,
                CPaisPrestacao = locPrest != null ? GetElementValue(locPrest, ns + "cPaisPrestacao") : string.Empty,
                CepPrestacao = string.Empty,

                CObra = obra != null ? GetElementValue(obra, ns + "cObra") : string.Empty,
                // 'art' não existe em TCInfoObra no schema v1.01
                Art = string.Empty,

                CTribNac = cServ != null ? GetElementValue(cServ, ns + "cTribNac") : string.Empty,
                CTribMun = cServ != null ? GetElementValue(cServ, ns + "cTribMun") : string.Empty,
                CNbs = cServ != null ? GetElementValue(cServ, ns + "cNBS") : string.Empty,
                XDescServ = cServ != null ? GetElementValue(cServ, ns + "xDescServ") : string.Empty,

                XOutInf = infoCompl != null ? GetElementValue(infoCompl, ns + "xOutInf") : string.Empty
            };

            if (trib != null)
            {
                var tribMun = trib.Element(ns + "tribMun");
                if (tribMun != null)
                {
                    data.TribIssqn = GetElementValue(tribMun, ns + "tribISSQN");
                    data.TpRetIssqn = GetElementValue(tribMun, ns + "tpRetISSQN");
                    
                    var bm = tribMun.Element(ns + "BM");
                    if (bm != null)
                    {
                        data.NBM = GetElementValue(bm, ns + "nBM");
                    }
                    
                    var exigSusp = tribMun.Element(ns + "exigSusp");
                    if (exigSusp != null)
                    {
                        data.TpSusp = GetElementValue(exigSusp, ns + "tpSusp");
                        data.NProcesso = GetElementValue(exigSusp, ns + "nProcesso");
                    }
                }

                var tribFed = trib.Element(ns + "tribFed");
                if (tribFed != null)
                {
                    data.VRetIrrf = GetElementValue(tribFed, ns + "vRetIRRF");
                    data.VRetCp = GetElementValue(tribFed, ns + "vRetCP");
                    data.VRetCsll = GetElementValue(tribFed, ns + "vRetCSLL");

                    var pisCofins = tribFed.Element(ns + "piscofins");
                    if (pisCofins != null)
                    {
                        data.VBCPisCofins = GetElementValue(pisCofins, ns + "vBCPisCofins");
                        data.PAliqPis = GetElementValue(pisCofins, ns + "pAliqPis");
                        data.PAliqCofins = GetElementValue(pisCofins, ns + "pAliqCofins");
                        data.VPis = GetElementValue(pisCofins, ns + "vPis");
                        data.VCofins = GetElementValue(pisCofins, ns + "vCofins");
                        data.TpRetPisCofins = GetElementValue(pisCofins, ns + "tpRetPisCofins");
                    }
                }
            }

            return data;
        }

        private static IbsCbsData ParseIbsCbs(XElement elem, XNamespace ns)
        {
            var data = new IbsCbsData();
            ParseIbsCbsElement(data, elem, ns);
            return data;
        }

        private static void ParseIbsCbsElement(IbsCbsData data, XElement elem, XNamespace ns)
        {
            if (elem == null) return;

            string cLoc = GetElementValue(elem, ns + "cLocalidadeIncid");
            if (!string.IsNullOrEmpty(cLoc)) data.CLocalidadeIncid = cLoc;

            string xLoc = GetElementValue(elem, ns + "xLocalidadeIncid");
            if (!string.IsNullOrEmpty(xLoc)) data.XLocalidadeIncid = xLoc;

            string cIndOp = GetElementValue(elem, ns + "cIndOp");
            if (!string.IsNullOrEmpty(cIndOp)) data.CIndOp = cIndOp;

            string finNFSe = GetElementValue(elem, ns + "finNFSe");
            if (!string.IsNullOrEmpty(finNFSe)) data.FinNFSe = finNFSe;

            var valores = elem.Element(ns + "valores");
            if (valores != null)
            {
                string vbc = GetElementValue(valores, ns + "vBC");
                if (!string.IsNullOrEmpty(vbc)) data.VBC = vbc;

                string vcalc = GetElementValue(valores, ns + "vCalcReeRepRes");
                if (!string.IsNullOrEmpty(vcalc)) data.VCalcReeRepRes = vcalc;

                var trib = valores.Element(ns + "trib");
                var gIBSCBS = trib != null ? trib.Element(ns + "gIBSCBS") : elem.Element(ns + "gIBSCBS");
                if (gIBSCBS != null)
                {
                    string cst = GetElementValue(gIBSCBS, ns + "CST");
                    if (!string.IsNullOrEmpty(cst)) data.Cst = cst;

                    string cclass = GetElementValue(gIBSCBS, ns + "cClassTrib");
                    if (!string.IsNullOrEmpty(cclass)) data.CClassTrib = cclass;
                }

                var uf = valores.Element(ns + "uf");
                if (uf != null)
                {
                    string pRedUF = GetElementValue(uf, ns + "pRedAliqUF");
                    if (!string.IsNullOrEmpty(pRedUF)) data.Uf.PRedAliqUF = pRedUF;

                    string pIbsUF = GetElementValue(uf, ns + "pIBSUF");
                    if (!string.IsNullOrEmpty(pIbsUF)) data.Uf.PIbsUF = pIbsUF;

                    string pAliqEfetUF = GetElementValue(uf, ns + "pAliqEfetUF");
                    if (!string.IsNullOrEmpty(pAliqEfetUF)) data.Uf.PAliqEfetUF = pAliqEfetUF;
                }

                var mun = valores.Element(ns + "mun");
                if (mun != null)
                {
                    string pRedMun = GetElementValue(mun, ns + "pRedAliqMun");
                    if (!string.IsNullOrEmpty(pRedMun)) data.Mun.PRedAliqMun = pRedMun;

                    string pIbsMun = GetElementValue(mun, ns + "pIBSMun");
                    if (!string.IsNullOrEmpty(pIbsMun)) data.Mun.PIbsMun = pIbsMun;

                    string pAliqEfetMun = GetElementValue(mun, ns + "pAliqEfetMun");
                    if (!string.IsNullOrEmpty(pAliqEfetMun)) data.Mun.PAliqEfetMun = pAliqEfetMun;
                }

                var fed = valores.Element(ns + "fed");
                if (fed != null)
                {
                    string pRedCBS = GetElementValue(fed, ns + "pRedAliqCBS");
                    if (!string.IsNullOrEmpty(pRedCBS)) data.Fed.PRedAliqCBS = pRedCBS;

                    string pCbs = GetElementValue(fed, ns + "pCBS");
                    if (!string.IsNullOrEmpty(pCbs)) data.Fed.PCbs = pCbs;

                    string pAliqEfetCBS = GetElementValue(fed, ns + "pAliqEfetCBS");
                    if (!string.IsNullOrEmpty(pAliqEfetCBS)) data.Fed.PAliqEfetCBS = pAliqEfetCBS;
                }
            }

            var totCIBS = elem.Element(ns + "totCIBS");
            if (totCIBS != null)
            {
                string vtot = GetElementValue(totCIBS, ns + "vTotNF");
                if (!string.IsNullOrEmpty(vtot)) data.VTotNF = vtot;

                var gIBS = totCIBS.Element(ns + "gIBS");
                if (gIBS != null)
                {
                    string vibsTot = GetElementValue(gIBS, ns + "vIBSTot");
                    if (!string.IsNullOrEmpty(vibsTot)) data.VIbsTot = vibsTot;

                    var gIbsUf = gIBS.Element(ns + "gIBSUFTot");
                    if (gIbsUf != null)
                    {
                        string vibsUf = GetElementValue(gIbsUf, ns + "vIBSUF");
                        if (!string.IsNullOrEmpty(vibsUf)) data.VIbsUf = vibsUf;
                    }

                    var gIbsMun = gIBS.Element(ns + "gIBSMunTot");
                    if (gIbsMun != null)
                    {
                        string vibsMun = GetElementValue(gIbsMun, ns + "vIBSMun");
                        if (!string.IsNullOrEmpty(vibsMun)) data.VIbsMun = vibsMun;
                    }
                }

                var gCBS = totCIBS.Element(ns + "gCBS");
                if (gCBS != null)
                {
                    string vcbs = GetElementValue(gCBS, ns + "vCBS");
                    if (!string.IsNullOrEmpty(vcbs)) data.VCbs = vcbs;
                }
            }
        }

        private static string GetElementValue(XElement parent, XName name)
        {
            return parent.Element(name)?.Value ?? string.Empty;
        }

        private static string GetAttributeValue(XElement elem, string attrName)
        {
            return elem.Attribute(attrName)?.Value ?? string.Empty;
        }
    }
}
