using System;
using System.IO;
using System.Text;
using System.Reflection;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using NFSe.DANFSe.v2.Models;
using NFSe.DANFSe.v2.Helpers;

namespace NFSe.DANFSe.v2.Rendering
{
    public class DanfsePdfRenderer
    {
        private readonly DanfseModel _model;
        private readonly byte[]? _logoBytes;
        private readonly PdfDocument _document;
        private readonly PdfPage _page;
        private readonly XGraphics _gfx;
        private readonly bool _forceTestWatermark;
        private readonly bool _autoFixInconsistentTotal;

        // Fontes Padrão
        private readonly XFont fontReg9;
        private readonly XFont fontBold9;
        private readonly XFont fontReg8;
        private readonly XFont fontBold8;
        private readonly XFont fontReg7;
        private readonly XFont fontBold7;
        private readonly XFont fontReg6;
        private readonly XFont fontBold6;

        // Estilos de Pen (canetas)
        private static readonly XPen BorderPen = new XPen(XColors.Black, 1.0);
        private static readonly XPen InnerPen = new XPen(XColors.Black, 0.5);

        // Estilos de Brush (pincéis)
        private static readonly XSolidBrush BlackBrush = new XSolidBrush(XColors.Black);
        private static readonly XSolidBrush RedBrush = new XSolidBrush(XColors.Red);
        private static readonly XSolidBrush ShadingBrush = new XSolidBrush(XColor.FromArgb(242, 242, 242));

        // Alinhamentos de Texto
        private static readonly XStringFormat LeftAlign = new XStringFormat { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.Near };
        private static readonly XStringFormat LeftCenterAlign = new XStringFormat { Alignment = XStringAlignment.Near, LineAlignment = XLineAlignment.Center };
        private static readonly XStringFormat CenterAlign = new XStringFormat { Alignment = XStringAlignment.Center, LineAlignment = XLineAlignment.Center };
        private static readonly XStringFormat RightAlign = new XStringFormat { Alignment = XStringAlignment.Far, LineAlignment = XLineAlignment.Center };

        private DanfsePdfRenderer(DanfseModel model, byte[]? logoBytes, bool forceTestWatermark, bool autoFixInconsistentTotal = false)
        {
            _model = model;
            _logoBytes = logoBytes;
            _forceTestWatermark = forceTestWatermark;
            _autoFixInconsistentTotal = autoFixInconsistentTotal;

            // Garante que o FontResolver customizado está registrado
            EmbeddedFontResolver.Register();

            _document = new PdfDocument();
            _page = _document.AddPage();
            _page.Size = PdfSharp.PageSize.A4;
            _gfx = XGraphics.FromPdfPage(_page);

            // Fontes Padrão
            fontReg9 = new XFont("LiberationSans", 9, XFontStyleEx.Regular);
            fontBold9 = new XFont("LiberationSans", 9, XFontStyleEx.Bold);
            fontReg8 = new XFont("LiberationSans", 8, XFontStyleEx.Regular);
            fontBold8 = new XFont("LiberationSans", 8, XFontStyleEx.Bold);
            fontReg7 = new XFont("LiberationSans", 7, XFontStyleEx.Regular);
            fontBold7 = new XFont("LiberationSans", 7, XFontStyleEx.Bold);
            fontReg6 = new XFont("LiberationSans", 6, XFontStyleEx.Regular);
            fontBold6 = new XFont("LiberationSans", 6, XFontStyleEx.Bold);
        }

        public static byte[] GeneratePdf(DanfseModel model, byte[]? logoBytes = null, bool forceTestWatermark = false, bool? autoFixInconsistentTotal = null)
        {
            // Se o parâmetro não foi fornecido (null), usar false como padrão
            // O chamador deve passar o valor lido do App.config
            bool shouldAutoFix = autoFixInconsistentTotal ?? false;

            var renderer = new DanfsePdfRenderer(model, logoBytes, forceTestWatermark, shouldAutoFix);
            renderer.Render();

            using (var stream = new MemoryStream())
            {
                renderer._document.Save(stream);
                return stream.ToArray();
            }
        }

        private static double CmToPt(double cm) => cm * 28.34645669;

        private void Render()
        {
            // Borda Externa da Página (0.15 cm de margem)
            _gfx.DrawRectangle(BorderPen, CmToPt(0.15), CmToPt(0.15), CmToPt(20.70), CmToPt(29.40));



            // ----------------------------------------------------------------------
            // 1. CABEÇALHO (Header)
            // ----------------------------------------------------------------------
            // Desenha retângulo sombreado do cabeçalho
            DrawShading(0.30, 0.30, 20.40, 1.17);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(1.47), CmToPt(20.70), CmToPt(1.47));

            // Carrega e exibe os logotipos oficiais e customizados (NT pág. 14)
            byte[] nfseLogoBytes = GetEmbeddedLogoBytes();
            bool hasCustomLogo = _logoBytes != null && _logoBytes.Length > 0 && !ByteArraysEqual(_logoBytes, nfseLogoBytes);

            // Se houver logotipo customizado, ele substitui completamente a logo nacional padrão no cabeçalho
            byte[] logoToDraw = hasCustomLogo ? _logoBytes! : nfseLogoBytes;
            DrawLogo(logoToDraw, 0.49, 0.30, 4.00, 1.16);

            // Textos Centrais do Cabeçalho
            DrawText("DANFSe v2.0", fontBold9, BlackBrush, 5.41, 0.35, 10.19, 0.35, CenterAlign);
            DrawText("Documento Auxiliar da NFS-e", fontBold9, BlackBrush, 5.41, 0.65, 10.19, 0.35, CenterAlign);

            // Homologação: NFS-e SEM VALIDADE JURÍDICA
            if (_model.TpAmb == Models.NfseSchemaValues.TpAmbHomologacao)
            {
                DrawText("NFS-e SEM VALIDADE JURÍDICA", fontBold9, RedBrush, 5.41, 0.95, 10.19, 0.35, CenterAlign);
            }

            // Identificação do Ambiente Gerador (NT Pág. 15)
            string ambGerStr = string.IsNullOrEmpty(_model.AmbGer) ? "-" : _model.AmbGer;

            // Identificação do Tipo de Ambiente (NT Pág. 15)
            string tpAmbStr = string.IsNullOrEmpty(_model.TpAmb) ? "-" : _model.TpAmb;

            // Textos da Direita do Cabeçalho
            string xLocEmi = Formatters.FormatMunUf(_model.XLocIncid, _model.CLocIncid, _model.Emitente.Endereco.UF);
            DrawText($"Município: {TruncateTextToWidth(xLocEmi, fontReg8, CmToPt(4.50))}", fontReg8, BlackBrush, 15.62, 0.45, 5.00, 0.35, LeftAlign);
            DrawText($"Ambiente Gerador: {TruncateTextToWidth(ambGerStr, fontReg6, CmToPt(4.50))}", fontReg6, BlackBrush, 15.62, 0.85, 5.00, 0.25, LeftAlign);
            DrawText($"Tipo de Ambiente: {TruncateTextToWidth(tpAmbStr, fontReg6, CmToPt(4.50))}", fontReg6, BlackBrush, 15.62, 1.15, 5.00, 0.25, LeftAlign);

            // ----------------------------------------------------------------------
            // 2. CHAVE DE ACESSO E DADOS DA NOTA
            // ----------------------------------------------------------------------
            DrawText("CHAVE DE ACESSO DA NFS-E", fontBold7, BlackBrush, 0.40, 1.55, 15.00, 0.30, LeftAlign);
            
            // Remove prefixo "NFS" se houver
            string chaveAcesso = _model.Id.StartsWith("NFS", StringComparison.OrdinalIgnoreCase) 
                ? _model.Id.Substring(3) 
                : _model.Id;
            DrawText(chaveAcesso, fontReg7, BlackBrush, 0.40, 1.90, 15.00, 0.30, LeftAlign);

            // QR Code (Desenho vetorial direto)
            string qrUrl = $"https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave={chaveAcesso}";
            QrCodeVectorDrawer.DrawQrCode(_gfx, qrUrl, CmToPt(17.48), CmToPt(1.60), CmToPt(1.90));

            // Instruções de Autenticidade
            DrawText("A autenticidade desta NFS-e pode ser verificada", fontReg6, BlackBrush, 15.62, 3.65, 5.00, 0.20, LeftAlign);
            DrawText("pela leitura deste código QR ou pela consulta da", fontReg6, BlackBrush, 15.62, 3.85, 5.00, 0.20, LeftAlign);
            DrawText("chave de acesso no portal nacional da NFS-e", fontReg6, BlackBrush, 15.62, 4.05, 5.00, 0.20, LeftAlign);

            // Dados Básicos da NFS-e
            DrawText("NÚMERO DA NFS-E", fontBold7, BlackBrush, 0.40, 2.35, 4.50, 0.25, LeftAlign);
            DrawText(_model.NNFSe, fontReg7, BlackBrush, 0.40, 2.65, 4.50, 0.25, LeftAlign);

            DrawText("COMPETÊNCIA DA NFS-E", fontBold7, BlackBrush, 5.41, 2.35, 4.50, 0.25, LeftAlign);
            DrawText(Formatters.FormatDate(_model.Dps.DCompet), fontReg7, BlackBrush, 5.41, 2.65, 4.50, 0.25, LeftAlign);

            DrawText("DATA E HORA DA EMISSÃO DA NFS-E", fontBold7, BlackBrush, 10.51, 2.35, 5.00, 0.25, LeftAlign);
            DrawText(Formatters.FormatDateTime(_model.DhProc), fontReg7, BlackBrush, 10.51, 2.65, 5.00, 0.25, LeftAlign);

            // Dados Básicos da DPS
            DrawText("NÚMERO DA DPS", fontBold7, BlackBrush, 0.40, 3.05, 4.50, 0.25, LeftAlign);
            DrawText(_model.Dps.NDPS, fontReg7, BlackBrush, 0.40, 3.35, 4.50, 0.25, LeftAlign);

            DrawText("SÉRIE DA DPS", fontBold7, BlackBrush, 5.41, 3.05, 4.50, 0.25, LeftAlign);
            DrawText(_model.Dps.Serie, fontReg7, BlackBrush, 5.41, 3.35, 4.50, 0.25, LeftAlign);

            DrawText("DATA E HORA DA EMISSÃO DA DPS", fontBold7, BlackBrush, 10.51, 3.05, 5.00, 0.25, LeftAlign);
            DrawText(Formatters.FormatDateTime(_model.Dps.DhEmi), fontReg7, BlackBrush, 10.51, 3.35, 5.00, 0.25, LeftAlign);

            // Bloco Emitente / Situação / Finalidade (Fundo Cinza)
            DrawShading(0.30, 3.67, 4.90, 0.67);
            DrawText("EMITENTE DA NFS-E", fontBold7, BlackBrush, 0.40, 3.72, 4.50, 0.25, LeftAlign);
            DrawText("SITUAÇÃO DA NFS-E", fontBold7, BlackBrush, 5.41, 3.72, 4.50, 0.25, LeftAlign);
            DrawText("FINALIDADE", fontBold7, BlackBrush, 10.51, 3.72, 5.00, 0.25, LeftAlign);

            DrawText(Formatters.FormatTpEmit(_model.Dps.TpEmit), fontReg7, BlackBrush, 0.40, 4.02, 4.50, 0.25, LeftAlign);
            DrawText(Formatters.FormatSituacaoNfse(_model.CStat), fontReg7, BlackBrush, 5.41, 4.02, 4.50, 0.25, LeftAlign);
            DrawText(Formatters.FormatFinalidadeNfse(_model.IbsCbs.FinNFSe), fontReg7, BlackBrush, 10.51, 4.02, 5.00, 0.25, LeftAlign);

            // ----------------------------------------------------------------------
            // 3. PRESTADOR / FORNECEDOR (Fixado)
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo (ficando por cima na camada de renderização)
            DrawShading(0.30, 4.34, 4.90, 0.64);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(4.34), CmToPt(20.70), CmToPt(4.34));

            DrawText("PRESTADOR / FORNECEDOR", fontBold7, BlackBrush, 0.40, 4.41, 4.50, 0.25, LeftAlign);
            
            DrawMetadataField("Prestador.Documento", Formatters.FormatDocumento(_model.Emitente.Documento));
            DrawMetadataField("Prestador.IM", _model.Prestador.IM);
            
            string prestFone = string.IsNullOrEmpty(_model.Emitente.Fone) ? _model.Prestador.Fone : _model.Emitente.Fone;
            DrawMetadataField("Prestador.Telefone", Formatters.FormatPhone(prestFone));

            // Linha 2
            DrawMetadataField("Prestador.Nome", _model.Emitente.Nome);
            
            string prestMun = Formatters.FormatMunUf(_model.Emitente.Endereco.XMun, _model.Emitente.Endereco.CMun, _model.Emitente.Endereco.UF);
            DrawMetadataField("Prestador.Municipio", prestMun);

            string prestIbgeCep = Formatters.FormatIbgeCep(_model.Emitente.Endereco.CMun, _model.Emitente.Endereco.CEP);
            DrawMetadataField("Prestador.IbgeCep", prestIbgeCep);

            // Linha 3
            string prestEnd = FormatEndereco(_model.Emitente.Endereco);
            DrawMetadataField("Prestador.Endereco", prestEnd);

            string prestEmail = string.IsNullOrEmpty(_model.Emitente.Email) ? _model.Prestador.Email : _model.Emitente.Email;
            DrawMetadataField("Prestador.Email", prestEmail);

            // Linha 4
            DrawMetadataField("Prestador.OpSimpNac", Formatters.FormatOpSimplesNacional(_model.Prestador.OpSimpNac));
            DrawMetadataField("Prestador.RegApTribSN", Formatters.FormatRegApTribSN(_model.Prestador.RegApTribSN));

            // ----------------------------------------------------------------------
            // POSICIONAMENTO DINÂMICO DOS BLOCOS DE TOMADOR, DESTINATÁRIO E INTERMEDIÁRIO
            // ----------------------------------------------------------------------
            double currentY = 6.92;
            const double AbsentHeight = 0.40; // Mínimo possível deixando 2px acima e abaixo

            // 4. TOMADOR / ADQUIRENTE
            if (!_model.Tomador.Identificado || (string.IsNullOrEmpty(_model.Tomador.Nome) && string.IsNullOrEmpty(_model.Tomador.Documento)))
            {
                // Desenha a linha de divisa por cima
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));
                DrawText("TOMADOR/ADQUIRENTE DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-E", fontReg7, BlackBrush, 0.30, currentY, 20.40, AbsentHeight, CenterAlign);
                currentY += AbsentHeight;
            }
            else
            {
                DrawShading(0.30, currentY, 4.90, 0.64);
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

                DrawText("TOMADOR / ADQUIRENTE", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);
                DrawMetadataField("Tomador.Documento", Formatters.FormatDocumento(_model.Tomador.Documento), currentY);
                DrawMetadataField("Tomador.IM", _model.Tomador.IM, currentY);
                DrawMetadataField("Tomador.Telefone", Formatters.FormatPhone(_model.Tomador.Fone), currentY);

                // Linha 2
                DrawMetadataField("Tomador.Nome", _model.Tomador.Nome, currentY);
                
                string tomaMun = Formatters.FormatMunUf(_model.Tomador.Endereco.XMun, _model.Tomador.Endereco.CMun, _model.Tomador.Endereco.UF);
                DrawMetadataField("Tomador.Municipio", tomaMun, currentY);

                string tomaIbgeCep = Formatters.FormatIbgeCep(_model.Tomador.Endereco.CMun, _model.Tomador.Endereco.CEP);
                DrawMetadataField("Tomador.IbgeCep", tomaIbgeCep, currentY);

                // Linha 3
                string tomaEnd = FormatEndereco(_model.Tomador.Endereco);
                DrawMetadataField("Tomador.Endereco", tomaEnd, currentY);
                DrawMetadataField("Tomador.Email", _model.Tomador.Email, currentY);

                currentY += 1.94;
            }

            // 5. DESTINATÁRIO DA OPERAÇÃO
            if (!_model.Destinatario.Identificado || (string.IsNullOrEmpty(_model.Destinatario.Nome) && string.IsNullOrEmpty(_model.Destinatario.Documento)))
            {
                string destText = "O DESTINATÁRIO É O PRÓPRIO TOMADOR/ADQUIRENTE DA OPERAÇÃO";
                if (!_model.Tomador.Identificado || (string.IsNullOrEmpty(_model.Tomador.Nome) && string.IsNullOrEmpty(_model.Tomador.Documento)))
                {
                    destText = "DESTINATÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-E";
                }
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));
                DrawText(destText, fontReg7, BlackBrush, 0.30, currentY, 20.40, AbsentHeight, CenterAlign);
                currentY += AbsentHeight;
            }
            else
            {
                DrawShading(0.30, currentY, 4.90, 0.64);
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

                DrawText("DESTINATÁRIO DA OPERAÇÃO", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);
                DrawMetadataField("Destinatario.Documento", Formatters.FormatDocumento(_model.Destinatario.Documento), currentY);
                DrawMetadataField("Destinatario.Telefone", Formatters.FormatPhone(_model.Destinatario.Fone), currentY);

                // Linha 2
                DrawMetadataField("Destinatario.Nome", _model.Destinatario.Nome, currentY);
                
                string destMun = Formatters.FormatMunUf(_model.Destinatario.Endereco.XMun, _model.Destinatario.Endereco.CMun, _model.Destinatario.Endereco.UF);
                DrawMetadataField("Destinatario.Municipio", destMun, currentY);

                string destIbgeCep = Formatters.FormatIbgeCep(_model.Destinatario.Endereco.CMun, _model.Destinatario.Endereco.CEP);
                DrawMetadataField("Destinatario.IbgeCep", destIbgeCep, currentY);

                // Linha 3
                string destEnd = FormatEndereco(_model.Destinatario.Endereco);
                DrawMetadataField("Destinatario.Endereco", destEnd, currentY);
                DrawMetadataField("Destinatario.Email", _model.Destinatario.Email, currentY);

                currentY += 1.94;
            }

            // 6. INTERMEDIÁRIO DA OPERAÇÃO
            if (string.IsNullOrEmpty(_model.Intermediario.Nome) && string.IsNullOrEmpty(_model.Intermediario.Documento))
            {
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));
                DrawText("INTERMEDIÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-E", fontReg7, BlackBrush, 0.30, currentY, 20.40, AbsentHeight, CenterAlign);
                currentY += AbsentHeight;
            }
            else
            {
                DrawShading(0.30, currentY, 4.90, 0.64);
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

                DrawText("INTERMEDIÁRIO DA OPERAÇÃO", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);
                DrawMetadataField("Interm.Documento", Formatters.FormatDocumento(_model.Intermediario.Documento), currentY);
                DrawMetadataField("Interm.IM", _model.Intermediario.IM, currentY);
                DrawMetadataField("Interm.Telefone", Formatters.FormatPhone(_model.Intermediario.Fone), currentY);

                // Linha 2
                DrawMetadataField("Interm.Nome", _model.Intermediario.Nome, currentY);
                
                string intMun = Formatters.FormatMunUf(_model.Intermediario.Endereco.XMun, _model.Intermediario.Endereco.CMun, _model.Intermediario.Endereco.UF);
                DrawMetadataField("Interm.Municipio", intMun, currentY);

                string intIbgeCep = Formatters.FormatIbgeCep(_model.Intermediario.Endereco.CMun, _model.Intermediario.Endereco.CEP);
                DrawMetadataField("Interm.IbgeCep", intIbgeCep, currentY);

                // Linha 3
                string intEnd = FormatEndereco(_model.Intermediario.Endereco);
                DrawMetadataField("Interm.Endereco", intEnd, currentY);
                DrawMetadataField("Interm.Email", _model.Intermediario.Email, currentY);

                currentY += 1.94;
            }

            // ----------------------------------------------------------------------
            // 7. SERVIÇO PRESTADO (Posicionamento dinâmico)
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
            DrawShading(0.30, currentY, 4.90, 0.64);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            DrawText("SERVIÇO PRESTADO", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);

            string tribCod = Formatters.FormatTribNac(_model.Servico.CTribNac);
            tribCod += " / " + (string.IsNullOrEmpty(_model.Servico.CTribMun) ? "-" : _model.Servico.CTribMun);
            DrawMetadataField("Servico.Tributacao", tribCod, currentY);

            string nbs = string.IsNullOrEmpty(_model.Servico.CNbs) ? "-" : Formatters.FormatNbs(_model.Servico.CNbs);
            DrawMetadataField("Servico.NBS", nbs, currentY);

            string localPrest = _model.Servico.XMunPrestacao;
            if (string.IsNullOrEmpty(localPrest))
            {
                localPrest = IbgeResolver.GetCityName(_model.Servico.CLocPrestacao);
            }

            if (!string.IsNullOrEmpty(localPrest) && localPrest != "-")
            {
                // Código IBGE resolvido: acrescenta UF e opcionalmente País
                string ufPrest = Formatters.GetUfFromIbge(_model.Servico.CLocPrestacao);
                if (!string.IsNullOrEmpty(ufPrest))
                {
                    localPrest += " / " + ufPrest;
                }
                string pais = (string.IsNullOrEmpty(_model.Servico.CPaisPrestacao) || _model.Servico.CPaisPrestacao == "1058") ? "-" : _model.Servico.CPaisPrestacao;
                localPrest += " / " + pais;
            }
            else if (!string.IsNullOrEmpty(_model.Servico.CLocPrestacao))
            {
                // Código de localidade especial (estrangeiro ou não-IBGE): exibe o código com contexto
                string ufPrest = Formatters.GetUfFromIbge(_model.Servico.CLocPrestacao);
                if (!string.IsNullOrEmpty(ufPrest))
                {
                    localPrest = $"Cód. {_model.Servico.CLocPrestacao} / {ufPrest}";
                }
                else
                {
                    localPrest = $"Cód. {_model.Servico.CLocPrestacao}";
                }
                string pais = (string.IsNullOrEmpty(_model.Servico.CPaisPrestacao) || _model.Servico.CPaisPrestacao == "1058") ? "BR" : _model.Servico.CPaisPrestacao;
                localPrest += " / " + pais;
            }
            else
            {
                localPrest = "-";
            }
            DrawMetadataField("Servico.LocalPrestacao", localPrest, currentY);

            // Descrição da Tributação Nacional
            string descTrib = string.IsNullOrEmpty(_model.XTribNac) ? "-" : _model.XTribNac;
            DrawText(descTrib, fontReg7, BlackBrush, 0.40, currentY + 0.72, 20.00, 0.30, LeftAlign);

            // Descrição do Serviço (com wrapping de texto automático e cálculo de altura dinâmica com pelo menos 6.3mm pós-texto)
            DrawText("Descrição do Serviço", fontBold6, BlackBrush, 0.40, currentY + 1.12, 20.00, 0.20, LeftAlign);
            
            double descServWidthPt = CmToPt(20.00);
            double descHeightPt = MeasureWrappedTextHeight(_model.Servico.XDescServ, fontReg7, descServWidthPt);
            double descHeightCm = descHeightPt / 28.34645669;

            DrawStringWrapped(_model.Servico.XDescServ, fontReg7, BlackBrush, 0.40, currentY + 1.31, 20.00, descHeightCm + 0.10);

            // Espaçamento mínimo pós-texto de 6.3mm (0.63 cm)
            currentY += 1.31 + descHeightCm + 0.63;

            // ----------------------------------------------------------------------
            // 8. TRIBUTAÇÃO MUNICIPAL (ISSQN)
            // ----------------------------------------------------------------------
            if (_model.Servico.TribIssqn == Models.NfseSchemaValues.TribIssqnNaoIncidencia || string.IsNullOrEmpty(_model.Servico.TribIssqn))
            {
                // Note 4: Para operações sem incidência do ISSQN, informa apenas a linha de não sujeita ao ISSQN
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));
                DrawText("TRIBUTAÇÃO MUNICIPAL (ISSQN) - OPERAÇÃO NÃO SUJEITA AO ISSQN", fontReg7, BlackBrush, 0.30, currentY, 20.40, AbsentHeight, CenterAlign);
                currentY += AbsentHeight;
            }
            else
            {
                // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
                DrawShading(0.30, currentY, 4.90, 0.64);
                _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

                DrawText("TRIBUTAÇÃO MUNICIPAL (ISSQN)", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);

                DrawMetadataField("Trib.TipoTributacao", Formatters.FormatTipoTributacao(_model.Servico.TribIssqn), currentY);

                string locIncidStr = Formatters.FormatMunUf(_model.XLocIncid, _model.CLocIncid, string.Empty);
                if (!string.IsNullOrEmpty(locIncidStr) && locIncidStr != "-")
                {
                    locIncidStr += " / BR";
                }
                DrawMetadataField("Trib.Incidencia", locIncidStr, currentY);

                // Linha 2
                DrawMetadataField("Trib.RegimeEspecial", Formatters.FormatRegimeEspecial(_model.Prestador.RegEspTrib), currentY);

                string tpImunidade = string.IsNullOrEmpty(_model.Servico.TpSusp) ? "-" : _model.Servico.TpSusp;
                DrawMetadataField("Trib.TipoImunidade", tpImunidade, currentY);
                DrawMetadataField("Trib.Suspensao", Formatters.FormatExigibilidadeSuspensa(_model.Servico.TpSusp), currentY);

                string nProcesso = string.IsNullOrEmpty(_model.Servico.NProcesso) ? "-" : _model.Servico.NProcesso;
                DrawMetadataField("Trib.NumeroProcesso", nProcesso, currentY);

                // Linha 3
                string bmDesc = Formatters.FormatTipoBM(_model.Valores.TpBM, _model.Versao);
                string nbm = _model.Servico.NBM;
                string bmText = "-";
                if (!string.IsNullOrEmpty(nbm) && !string.IsNullOrEmpty(bmDesc))
                {
                    bmText = $"{nbm} - {bmDesc}";
                }
                else if (!string.IsNullOrEmpty(nbm))
                {
                    bmText = nbm;
                }
                else if (!string.IsNullOrEmpty(bmDesc))
                {
                    bmText = bmDesc;
                }
                DrawMetadataField("Trib.BeneficioMunicipal", bmText, currentY);

                string calcBm = string.IsNullOrEmpty(_model.Valores.VCalcBM) ? "-" : Formatters.FormatCurrency(_model.Valores.VCalcBM);
                DrawMetadataField("Trib.CalculoBM", calcBm, currentY);

                string totalDed = string.IsNullOrEmpty(_model.Valores.VCalcDR) ? "-" : _model.Valores.VCalcDR;
                DrawMetadataField("Trib.Deducoes", totalDed, currentY);
                DrawMetadataField("Trib.DescontoIncondicionado", "-", currentY);

                // Linha 4
                DrawMetadataField("Trib.BcIssqn", Formatters.FormatCurrency(_model.Valores.VBC), currentY);
                DrawMetadataField("Trib.AliquotaAplicada", string.IsNullOrEmpty(_model.Valores.PAliqAplic) ? "-" : Formatters.FormatPercent(_model.Valores.PAliqAplic), currentY);
                DrawMetadataField("Trib.RetencaoIssqn", Formatters.FormatRetIss(_model.Servico.TpRetIssqn), currentY);
                DrawMetadataField("Trib.IssqnApurado", Formatters.FormatCurrency(_model.Valores.VIssqn), currentY);

                currentY += 2.59;
            }

            // ----------------------------------------------------------------------
            // 9. TRIBUTAÇÃO FEDERAL (EXCETO CBS)
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
            DrawShading(0.30, currentY, 4.90, 0.64);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            DrawText("TRIBUTAÇÃO FEDERAL (EXCETO CBS)", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);

            // Linha 1: BC PIS/COFINS e Alíquotas
            string bcPisCofins = string.IsNullOrEmpty(_model.Servico.VBCPisCofins) ? "-" : Formatters.FormatCurrency(_model.Servico.VBCPisCofins);
            DrawMetadataField("Fed.VBCPisCofins", bcPisCofins, currentY);

            string aliqPis = string.IsNullOrEmpty(_model.Servico.PAliqPis) ? "-" : _model.Servico.PAliqPis;
            DrawMetadataField("Fed.AliqPis", aliqPis, currentY);

            string aliqCofins = string.IsNullOrEmpty(_model.Servico.PAliqCofins) ? "-" : _model.Servico.PAliqCofins;
            DrawMetadataField("Fed.AliqCofins", aliqCofins, currentY);

            // Linha 2: IRRF, CP, CSLL
            string irrf = string.IsNullOrEmpty(_model.Servico.VRetIrrf) ? "-" : Formatters.FormatCurrency(_model.Servico.VRetIrrf);
            DrawMetadataField("Fed.IRRF", irrf, currentY);

            string cp = string.IsNullOrEmpty(_model.Servico.VRetCp) ? "-" : Formatters.FormatCurrency(_model.Servico.VRetCp);
            DrawMetadataField("Fed.CP", cp, currentY);

            string csll = string.IsNullOrEmpty(_model.Servico.VRetCsll) ? "-" : Formatters.FormatCurrency(_model.Servico.VRetCsll);
            DrawMetadataField("Fed.CSLL", csll, currentY);

            // Linha 3: PIS, COFINS, Descrição Retenção
            string pis = string.IsNullOrEmpty(_model.Servico.VPis) ? "-" : Formatters.FormatCurrency(_model.Servico.VPis);
            DrawMetadataField("Fed.PIS", pis, currentY);

            string cofins = string.IsNullOrEmpty(_model.Servico.VCofins) ? "-" : Formatters.FormatCurrency(_model.Servico.VCofins);
            DrawMetadataField("Fed.COFINS", cofins, currentY);

            DrawMetadataField("Fed.RetPisCofins", Formatters.FormatRetPisCofins(_model.Servico.TpRetPisCofins), currentY);

            currentY += 1.95;

            // ----------------------------------------------------------------------
            // 10. TRIBUTAÇÃO IBS / CBS
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
            DrawShading(0.30, currentY, 4.90, 0.64);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            DrawText("TRIBUTAÇÃO IBS / CBS", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);

            DrawText("CST / CCLASS TRIB", fontBold6, BlackBrush, 5.41, currentY + 0.07, 4.50, 0.20, LeftAlign);
            string cstCClassStr = Formatters.FormatCstCClassTrib(_model.IbsCbs.Cst, _model.IbsCbs.CClassTrib);
            DrawText(cstCClassStr, fontReg7, BlackBrush, 5.41, currentY + 0.37, 4.50, 0.25, LeftAlign);

            DrawText("Indicador Op. / IBGE Incidência / Município Incidência / UF", fontBold6, BlackBrush, 10.51, currentY + 0.07, 9.50, 0.20, LeftAlign);
            string ufIbs = Formatters.GetUfFromIbge(_model.IbsCbs.CLocalidadeIncid);
            string indOpStr = Formatters.FormatIbsCbsIndOp(_model.IbsCbs.CIndOp, _model.IbsCbs.CLocalidadeIncid, _model.IbsCbs.XLocalidadeIncid, ufIbs);
            DrawText(indOpStr, fontReg7, BlackBrush, 10.51, currentY + 0.37, 9.50, 0.25, LeftAlign);

            // Linha 2
            DrawText("Exclusões e Reduções da Base de Cálculo", fontBold6, BlackBrush, 0.40, currentY + 0.71, 4.50, 0.20, LeftAlign);
            
            // Somatório de vDescIncond + vCalcReeRepRes + vISSQN + vPIS + vCOFINS conforme especificação oficial do DANFSe v2.0
            double sumExclusoes = 0;
            if (double.TryParse(_model.IbsCbs.VCalcReeRepRes, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vCalcReeRepRes)) sumExclusoes += vCalcReeRepRes;
            if (double.TryParse(_model.Valores.VIssqn, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vIssqn)) sumExclusoes += vIssqn;
            if (double.TryParse(_model.Servico.VPis, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vPis)) sumExclusoes += vPis;
            if (double.TryParse(_model.Servico.VCofins, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vCofins)) sumExclusoes += vCofins;

            string exclusoesStr = sumExclusoes > 0 ? sumExclusoes.ToString("C2", Helpers.Formatters.PtBrCurrency) : "-";
            DrawText(exclusoesStr, fontReg7, BlackBrush, 0.40, currentY + 1.01, 4.50, 0.25, LeftAlign);

            DrawText("Base de Cálculo Após Exclusões e Reduções", fontBold6, BlackBrush, 5.41, currentY + 0.71, 4.50, 0.20, LeftAlign);
            string bcIbs = string.IsNullOrEmpty(_model.IbsCbs.VBC) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VBC);
            DrawText(bcIbs, fontReg7, BlackBrush, 5.41, currentY + 1.01, 4.50, 0.25, LeftAlign);

            DrawText("Red. Alíquota IBS / Red. Alíquota CBS", fontBold6, BlackBrush, 10.51, currentY + 0.71, 4.50, 0.20, LeftAlign);
            string redUf = string.IsNullOrEmpty(_model.IbsCbs.Uf.PRedAliqUF) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Uf.PRedAliqUF);
            string redMun = string.IsNullOrEmpty(_model.IbsCbs.Mun.PRedAliqMun) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Mun.PRedAliqMun);
            string redFed = string.IsNullOrEmpty(_model.IbsCbs.Fed.PRedAliqCBS) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Fed.PRedAliqCBS);
            string redAliq = (redUf == "-" && redMun == "-" && redFed == "-") ? "- / - / -" : $"{redUf} / {redMun} / {redFed}";
            DrawText(redAliq, fontReg7, BlackBrush, 10.51, currentY + 1.01, 4.50, 0.25, LeftAlign);

            DrawText("Alíquota - IBS UF / IBS MUN", fontBold6, BlackBrush, 15.62, currentY + 0.71, 4.50, 0.20, LeftAlign);
            string pIbsUfStr = string.IsNullOrEmpty(_model.IbsCbs.Uf.PIbsUF) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Uf.PIbsUF);
            string pIbsMunStr = string.IsNullOrEmpty(_model.IbsCbs.Mun.PIbsMun) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Mun.PIbsMun);
            string aliqIbs = $"{pIbsUfStr} / {pIbsMunStr}";
            DrawText(aliqIbs, fontReg7, BlackBrush, 15.62, currentY + 1.01, 4.50, 0.25, LeftAlign);

            // Linha 3
            DrawText("Alíq. Efetiva Municipal - IBS", fontBold6, BlackBrush, 0.40, currentY + 1.36, 4.50, 0.20, LeftAlign);
            string aliqEfetMun = string.IsNullOrEmpty(_model.IbsCbs.Mun.PAliqEfetMun) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Mun.PAliqEfetMun);
            DrawText(aliqEfetMun, fontReg7, BlackBrush, 0.40, currentY + 1.66, 4.50, 0.25, LeftAlign);

            DrawText("Valor Apurado Municipal - IBS", fontBold6, BlackBrush, 5.41, currentY + 1.36, 4.50, 0.20, LeftAlign);
            string vIbsMun = string.IsNullOrEmpty(_model.IbsCbs.VIbsMun) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VIbsMun);
            DrawText(vIbsMun, fontReg7, BlackBrush, 5.41, currentY + 1.66, 4.50, 0.25, LeftAlign);

            DrawText("Alíq. Efetiva Estadual - IBS", fontBold6, BlackBrush, 10.51, currentY + 1.36, 4.50, 0.20, LeftAlign);
            string aliqEfetUf = string.IsNullOrEmpty(_model.IbsCbs.Uf.PAliqEfetUF) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Uf.PAliqEfetUF);
            DrawText(aliqEfetUf, fontReg7, BlackBrush, 10.51, currentY + 1.66, 4.50, 0.25, LeftAlign);

            DrawText("Valor Apurado Estadual - IBS", fontBold6, BlackBrush, 15.62, currentY + 1.36, 4.50, 0.20, LeftAlign);
            string vIbsUf = string.IsNullOrEmpty(_model.IbsCbs.VIbsUf) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VIbsUf);
            DrawText(vIbsUf, fontReg7, BlackBrush, 15.62, currentY + 1.66, 4.50, 0.25, LeftAlign);

            // Linha 4
            DrawText("Valor Total Apurado - IBS", fontBold6, BlackBrush, 0.40, currentY + 2.01, 4.50, 0.20, LeftAlign);
            string vIbsTot = string.IsNullOrEmpty(_model.IbsCbs.VIbsTot) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VIbsTot);
            DrawText(vIbsTot, fontReg7, BlackBrush, 0.40, currentY + 2.31, 4.50, 0.25, LeftAlign);

            DrawText("Alíquota - CBS", fontBold6, BlackBrush, 5.41, currentY + 2.01, 4.50, 0.20, LeftAlign);
            string pCbs = string.IsNullOrEmpty(_model.IbsCbs.Fed.PCbs) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Fed.PCbs);
            DrawText(pCbs, fontReg7, BlackBrush, 5.41, currentY + 2.31, 4.50, 0.25, LeftAlign);

            DrawText("Alíquota Efetiva - CBS", fontBold6, BlackBrush, 10.51, currentY + 2.01, 4.50, 0.20, LeftAlign);
            string aliqEfetCbs = string.IsNullOrEmpty(_model.IbsCbs.Fed.PAliqEfetCBS) ? "-" : Formatters.FormatPercent(_model.IbsCbs.Fed.PAliqEfetCBS);
            DrawText(aliqEfetCbs, fontReg7, BlackBrush, 10.51, currentY + 2.31, 4.50, 0.25, LeftAlign);

            DrawText("Valor Total Apurado - CBS", fontBold6, BlackBrush, 15.62, currentY + 2.01, 4.50, 0.20, LeftAlign);
            string vCbs = string.IsNullOrEmpty(_model.IbsCbs.VCbs) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VCbs);
            DrawText(vCbs, fontReg7, BlackBrush, 15.62, currentY + 2.31, 4.50, 0.25, LeftAlign);

            currentY += 2.58;

            // ----------------------------------------------------------------------
            // 11. VALOR TOTAL DA NFS-E
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
            DrawShading(0.30, currentY, 4.90, 0.67);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            DrawText("VALOR TOTAL DA NFS-E", fontBold7, BlackBrush, 0.40, currentY + 0.07, 4.50, 0.25, LeftAlign);

            DrawText("Valor da Operação / Serviço", fontBold6, BlackBrush, 5.41, currentY + 0.07, 4.50, 0.20, LeftAlign);
            DrawText(Formatters.FormatCurrency(_model.Valores.VLiq), fontReg7, BlackBrush, 5.41, currentY + 0.37, 4.50, 0.25, LeftAlign);

            DrawText("Desconto Incondicionado", fontBold6, BlackBrush, 10.51, currentY + 0.07, 4.50, 0.20, LeftAlign);
            DrawText("-", fontReg7, BlackBrush, 10.51, currentY + 0.37, 4.50, 0.25, LeftAlign);

            DrawText("Desconto Condicionado", fontBold6, BlackBrush, 15.62, currentY + 0.07, 4.50, 0.20, LeftAlign);
            DrawText("-", fontReg7, BlackBrush, 15.62, currentY + 0.37, 4.50, 0.25, LeftAlign);

            // Linha 2
            DrawText("Total das Retenções (ISSQN / Federais)", fontBold6, BlackBrush, 0.40, currentY + 0.76, 4.50, 0.20, LeftAlign);
            string totRet = string.IsNullOrEmpty(_model.Valores.VTotalRet) ? "-" : Formatters.FormatCurrency(_model.Valores.VTotalRet);
            DrawText(totRet, fontReg7, BlackBrush, 0.40, currentY + 1.06, 4.50, 0.25, LeftAlign);

            DrawText("Valor Líquido da NFS-e", fontBold6, BlackBrush, 5.41, currentY + 0.76, 4.50, 0.20, LeftAlign);
            DrawText(Formatters.FormatCurrency(_model.Valores.VLiq), fontReg7, BlackBrush, 5.41, currentY + 1.06, 4.50, 0.25, LeftAlign);

            DrawText("Total do IBS/CBS", fontBold6, BlackBrush, 10.51, currentY + 0.76, 4.50, 0.20, LeftAlign);
            double.TryParse(_model.IbsCbs.VIbsTot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double ibsVal);
            double.TryParse(_model.IbsCbs.VCbs, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cbsVal);
            double totIbsCbsVal = ibsVal + cbsVal;
            string totIbsCbs = totIbsCbsVal > 0 ? totIbsCbsVal.ToString("C2", Helpers.Formatters.PtBrCurrency) : "-";
            DrawText(totIbsCbs, fontReg7, BlackBrush, 10.51, currentY + 1.06, 4.50, 0.25, LeftAlign);

            DrawText("Valor Líquido da NFS-e + IBS/CBS", fontBold6, BlackBrush, 15.62, currentY + 0.76, 4.50, 0.20, LeftAlign);
            string totNf;
            if (_autoFixInconsistentTotal && totIbsCbsVal > 0)
            {
                double.TryParse(_model.Valores.VLiq, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vLiqVal);
                double.TryParse(_model.IbsCbs.VTotNF, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vTotNfXmlVal);
                double vTotNfEsperado = vLiqVal + totIbsCbsVal;

                // Se o XML veio omitindo o IBS/CBS por fora (vTotNfXmlVal == vLiqVal < vTotNfEsperado), ajusta dinamicamente
                if (Math.Abs(vTotNfXmlVal - vLiqVal) < 0.01 && vTotNfXmlVal < (vTotNfEsperado - 0.001))
                {
                    totNf = vTotNfEsperado.ToString("C2", Helpers.Formatters.PtBrCurrency);
                }
                else
                {
                    totNf = string.IsNullOrEmpty(_model.IbsCbs.VTotNF) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VTotNF);
                }
            }
            else
            {
                totNf = string.IsNullOrEmpty(_model.IbsCbs.VTotNF) ? "-" : Formatters.FormatCurrency(_model.IbsCbs.VTotNF);
            }
            DrawText(totNf, fontReg7, BlackBrush, 15.62, currentY + 1.06, 4.50, 0.25, LeftAlign);

            currentY += 1.37;
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            // ----------------------------------------------------------------------
            // 12. INFORMAÇÕES COMPLEMENTARES
            // ----------------------------------------------------------------------
            // Desenha shading primeiro, depois a linha preta horizontal divisória no topo
            DrawShading(0.30, currentY, 4.90, 0.64);
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(currentY), CmToPt(20.70), CmToPt(currentY));

            DrawText("INFORMAÇÕES COMPLEMENTARES", fontBold7, BlackBrush, 0.40, currentY + 0.07, 20.00, 0.25, LeftAlign);

            // Outras informações complementares apresentadas logo abaixo do retângulo cinza de indicação do dado
            string outInf = BuildComplementaryInfo();
            if (!string.IsNullOrEmpty(outInf) && outInf != "-")
            {
                double generalMaxHeight = 27.60 - (currentY + 0.70);
                if (generalMaxHeight > 0.30)
                {
                    DrawStringWrapped(outInf, fontReg7, BlackBrush, 0.40, currentY + 0.70, 20.00, generalMaxHeight);
                }
            }

            // Linha de totalização sempre ancorada embaixo (Bottom), logo acima do Canhoto
            string totalsText = BuildTotalsTributosInfo();
            DrawText(totalsText, fontReg7, BlackBrush, 0.40, 27.70, 20.00, 0.35, LeftAlign);

            // ----------------------------------------------------------------------
            // 13. CANHOTO (DATA CIENTIFICAÇÃO E ASSINATURA)
            // ----------------------------------------------------------------------
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(28.10), CmToPt(20.70), CmToPt(28.10));
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(28.77), CmToPt(20.70), CmToPt(28.77));
            _gfx.DrawLine(InnerPen, CmToPt(0.30), CmToPt(28.10), CmToPt(0.30), CmToPt(28.77));
            _gfx.DrawLine(InnerPen, CmToPt(20.70), CmToPt(28.10), CmToPt(20.70), CmToPt(28.77));

            // Divisórias verticais do canhoto
            _gfx.DrawLine(InnerPen, CmToPt(5.41), CmToPt(28.10), CmToPt(5.41), CmToPt(28.77));
            _gfx.DrawLine(InnerPen, CmToPt(10.51), CmToPt(28.10), CmToPt(10.51), CmToPt(28.77));

            DrawText("DATA CIENTIFICAÇÃO:", fontBold6, BlackBrush, 0.40, 28.17, 4.50, 0.20, LeftAlign);
            DrawText("IDENTIFICAÇÃO E ASSINATURA", fontBold6, BlackBrush, 5.51, 28.17, 4.50, 0.20, LeftAlign);
            DrawText("Nº NFS-e / CHAVE NFS-e", fontBold6, BlackBrush, 10.61, 28.17, 9.50, 0.20, LeftAlign);

            DrawText($"{_model.NNFSe} / {chaveAcesso}", fontReg6, BlackBrush, 10.61, 28.47, 9.50, 0.20, LeftAlign);

            // ----------------------------------------------------------------------
            // 14. MARCAS D'ÁGUA DE CANCELAMENTO / SUBSTITUIÇÃO (NT Pág. 23)
            // ----------------------------------------------------------------------
            if (_model.IsCancelled || _model.IsSubstituted || _forceTestWatermark)
            {
                string watermarkText = _forceTestWatermark ? "TESTE" : (_model.IsCancelled ? "CANCELADA" : "SUBSTITUÍDA");
                
                var state = _gfx.Save();

                // Centraliza a rotação no meio da folha A4
                double centerX = CmToPt(21.0 / 2.0);
                double centerY = CmToPt(29.7 / 2.0);

                _gfx.TranslateTransform(centerX, centerY);
                _gfx.RotateTransform(-35); // Ângulo diagonal de subida

                // Fonte tamanho 50 em formato normal (Regular)
                var watermarkFont = new XFont("LiberationSans", 50, XFontStyleEx.Regular);
                
                // Cinza K35 (RGB 166, 166, 166) semi-transparente (Opacidade 30% = 76 em 255)
                var watermarkBrush = new XSolidBrush(XColor.FromArgb(76, 166, 166, 166));

                _gfx.DrawString(watermarkText, watermarkFont, watermarkBrush, new XPoint(0, 0), CenterAlign);

                _gfx.Restore(state);
            }
        }

        private static byte[] GetEmbeddedLogoBytes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "NFSe.DANFSe.v2.Resources.Images.logo-nfs-e-horizontal.png";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return Array.Empty<byte>();
                }
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private void DrawLogo(byte[]? logoBytes, double x, double y, double w, double h)
        {
            if (logoBytes == null || logoBytes.Length == 0) return;
            try
            {
                using (var ms = new MemoryStream(logoBytes, 0, logoBytes.Length, false, true))
                {
                    using (var img = XImage.FromStream(ms))
                    {
                        double logoW = CmToPt(w);
                        double logoH = CmToPt(h);
                        double ratio = (double)img.PixelHeight / img.PixelWidth;
                        double actualH = logoW * ratio;

                        double logoY = CmToPt(y);
                        if (actualH < logoH)
                        {
                            logoY += (logoH - actualH) / 2.0;
                        }
                        else
                        {
                            actualH = logoH;
                        }

                        _gfx.DrawImage(img, CmToPt(x), logoY, logoW, actualH);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"[NFSe.DANFSe.v2] Erro ao desenhar logotipo no PDF: {ex.Message}");
            }
        }

        private string TruncateTextToWidth(string text, XFont font, double maxWidthPt)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            
            double textWidth = _gfx.MeasureString(text, font).Width;
            if (textWidth <= maxWidthPt) return text;
            
            string ellipsis = "...";
            double ellipsisWidth = _gfx.MeasureString(ellipsis, font).Width;
            if (ellipsisWidth >= maxWidthPt) return string.Empty;
            
            int low = 0;
            int high = text.Length;
            int bestLength = 0;
            
            while (low <= high)
            {
                int mid = (low + high) / 2;
                string sub = text.Substring(0, mid) + ellipsis;
                double subWidth = _gfx.MeasureString(sub, font).Width;
                
                if (subWidth <= maxWidthPt)
                {
                    bestLength = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            
            return text.Substring(0, bestLength) + ellipsis;
        }

        private string TruncateTextToWidth(string text, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxLength) return text;
            
            if (maxLength <= ellipsis.Length)
            {
                return text.Substring(0, maxLength);
            }
            
            return text.Substring(0, maxLength - ellipsis.Length) + ellipsis;
        }

        private void DrawShading(double x, double y, double w, double h)
        {
            _gfx.DrawRectangle(ShadingBrush, CmToPt(x), CmToPt(y), CmToPt(w), CmToPt(h));
        }

        private void DrawText(string text, XFont font, XBrush brush, double x, double y, double w, double h, XStringFormat format)
        {
            if (string.IsNullOrEmpty(text)) return;
            var rect = new XRect(CmToPt(x), CmToPt(y), CmToPt(w), CmToPt(h));
            _gfx.DrawString(text, font, brush, rect, format);
        }

        private XStringFormat MapAlignment(LayoutAlignment alignment)
        {
            switch (alignment)
            {
                case LayoutAlignment.Center: return CenterAlign;
                case LayoutAlignment.Right: return RightAlign;
                default: return LeftAlign;
            }
        }

        private void DrawMetadataField(string fieldKey, string value, double currentBlockY = 0)
        {
            if (!DanfseLayoutRegistry.Fields.TryGetValue(fieldKey, out var meta)) return;

            double absoluteY = currentBlockY + meta.YOffset;

            // 1. Desenha o fundo cinza se configurado
            if (meta.HasShading)
            {
                DrawShading(meta.XOffset, absoluteY, meta.Width, meta.Height);
            }

            // 2. Desenha a label de identificação no topo da célula se aplicável
            if (!string.IsNullOrEmpty(meta.Label))
            {
                DrawText(meta.Label, fontBold6, BlackBrush, meta.XOffset, absoluteY, meta.Width, 0.20, LeftAlign);
            }

            // 3. Trunca o texto conforme limite de caracteres ou largura física
            string textToDraw = value;
            if (meta.MaxLength > 0)
            {
                textToDraw = TruncateTextToWidth(textToDraw, meta.MaxLength);
            }
            else
            {
                textToDraw = TruncateTextToWidth(textToDraw, fontReg7, CmToPt(meta.Width - 0.20));
            }

            // 4. Desenha o valor de fato
            double valueYOffset = !string.IsNullOrEmpty(meta.Label) ? 0.30 : 0.0;
            DrawText(textToDraw, fontReg7, BlackBrush, meta.XOffset, absoluteY + valueYOffset, meta.Width, meta.Height, MapAlignment(meta.Alignment));
        }

        private void DrawStringWrapped(string text, XFont font, XBrush brush, double x, double y, double w, double h)
        {
            if (string.IsNullOrEmpty(text)) return;
            var rect = new XRect(CmToPt(x), CmToPt(y), CmToPt(w), CmToPt(h));
            var tf = new XTextFormatter(_gfx);
            tf.DrawString(text, font, brush, rect);
        }

        private string FormatEndereco(Endereco ender)
        {
            var sb = new StringBuilder();
            sb.Append(ender.Logradouro);
            
            if (!string.IsNullOrEmpty(ender.Numero))
            {
                sb.Append($", {ender.Numero}");
            }
            if (!string.IsNullOrEmpty(ender.Complemento))
            {
                sb.Append($" - {ender.Complemento}");
            }
            if (!string.IsNullOrEmpty(ender.Bairro))
            {
                sb.Append($", {ender.Bairro}");
            }

            return sb.ToString();
        }

        private string BuildComplementaryInfo()
        {
            var sb = new StringBuilder();
            
            // 1. Inf. Cont. (Informações Complementares)
            if (!string.IsNullOrEmpty(_model.Servico.XOutInf) && _model.Servico.XOutInf != "-")
            {
                sb.Append($"Inf. Cont.: {_model.Servico.XOutInf}");
            }
            
            // 2. NFS-e Subst. (Nota 7)
            if (_model.IsSubstituted)
            {
                if (sb.Length > 0) sb.Append(" | ");
                sb.Append("NFS-e Subst.: [Substituída]");
            }
            
            // 4. Cod. Obra (Nota 8)
            if (!string.IsNullOrEmpty(_model.Servico.CObra) && _model.Servico.CObra != "-")
            {
                if (sb.Length > 0) sb.Append(" | ");
                sb.Append($"Cod. Obra: {_model.Servico.CObra}");
            }

            string result = sb.ToString();
            result = TruncateTextToWidth(result, 1997);
            return result;
        }

        // A NT 008 (tabela de leiaute) nomeia a fonte destes valores: vTotTribFed/Est/Mun — o grupo totTrib da
        // Lei 12.741/2012, calculado pela tabela IBPT. Retenções e ISS são outras grandezas: numa nota com
        // vTotTribMun=22,73 e ISS=15,00, imprimir o ISS declara valor errado num campo exigido por lei.
        // A NT 008 (tabela de leiaute e Nota 10, pág. 21) nomeia a fonte destes valores:
        // vTotTribFed/Est/Mun ou pTotTribFed/Est/Mun — o grupo totTrib da Lei 12.741/2012 (IBPT).
        // A informação pode ser preenchida com valores monetários OU percentuais:
        // "Totais Aproximados dos Tributos cfe. Lei nº 12.741/2012: Federais: R$ ou % ; Estaduais: R$ ou % ; Municipais: R$ ou %"
        private string BuildTotalsTributosInfo()
        {
            static string FormatTribValue(string? valorMonetario, string? percentual)
            {
                if (double.TryParse(valorMonetario, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v) && v > 0)
                {
                    return v.ToString("C2", Helpers.Formatters.PtBrCurrency);
                }

                if (double.TryParse(percentual, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p) && p > 0)
                {
                    return Helpers.Formatters.FormatPercent(percentual);
                }

                return "-";
            }

            string fedStr = FormatTribValue(_model.Valores.VTotTribFed, _model.Valores.PTotTribFed);
            string estStr = FormatTribValue(_model.Valores.VTotTribEst, _model.Valores.PTotTribEst);
            string munStr = FormatTribValue(_model.Valores.VTotTribMun, _model.Valores.PTotTribMun);

            return "Totais Aproximados dos Tributos cfe. Lei nº 12.741/2012: "
                + $"Federais: {fedStr} ; "
                + $"Estaduais: {estStr} ; "
                + $"Municipais: {munStr}";
        }

        private static bool ByteArraysEqual(byte[]? b1, byte[]? b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
        private double MeasureWrappedTextHeight(string text, XFont font, double maxWidthPt)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            string[] paragraphLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            double lineHeightPt = font.Height;
            int totalLines = 0;

            foreach (var paragraph in paragraphLines)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    totalLines++;
                    continue;
                }

                string[] words = paragraph.Split(' ');
                string currentLine = "";

                foreach (var word in words)
                {
                    string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                    double width = _gfx.MeasureString(testLine, font).Width;

                    if (width <= maxWidthPt)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                        {
                            totalLines++;
                        }
                        currentLine = word;
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    totalLines++;
                }
            }

            return totalLines * lineHeightPt;
        }
    }
}
