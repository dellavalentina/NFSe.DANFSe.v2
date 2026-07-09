using System;
using System.IO;
using System.Reflection;
using PdfSharp.Fonts;

namespace NFSe.DANFSe.v2.Rendering
{
    public class EmbeddedFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("LiberationSans", StringComparison.OrdinalIgnoreCase) ||
                familyName.Equals("Liberation Sans", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold)
                {
                    return new FontResolverInfo("LiberationSans-Bold");
                }
                return new FontResolverInfo("LiberationSans-Regular");
            }
            
            return null;
        }

        public byte[] GetFont(string faceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // O nome do recurso embutido depende do namespace padrão (NFSe.DANFSe.v2) e o caminho físico
            string resourceName = faceName switch
            {
                "LiberationSans-Regular" => "NFSe.DANFSe.v2.Resources.Fonts.LiberationSans-Regular.ttf",
                "LiberationSans-Bold" => "NFSe.DANFSe.v2.Resources.Fonts.LiberationSans-Bold.ttf",
                _ => throw new ArgumentException($"Fonte face '{faceName}' não suportada.")
            };

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    var names = string.Join(", ", assembly.GetManifestResourceNames());
                    throw new FileNotFoundException($"Recurso '{resourceName}' não encontrado no Assembly. Recursos disponíveis: {names}");
                }

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static void Register()
        {
            try
            {
                // Evita lançar exceção caso já tenha sido registrado anteriormente em testes
                if (!(GlobalFontSettings.FontResolver is EmbeddedFontResolver))
                {
                    GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
                }
            }
            catch (InvalidOperationException)
            {
                // Já registrado
            }
        }
    }
}
