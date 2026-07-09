using System;
using PdfSharp.Drawing;
using QRCoder;

namespace NFSe.DANFSe.v2.Rendering
{
    public static class QrCodeVectorDrawer
    {
        public static void DrawQrCode(XGraphics graphics, string text, double xPt, double yPt, double sizePt)
        {
            if (string.IsNullOrEmpty(text)) return;

            using (var qrGenerator = new QRCodeGenerator())
            {
                using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M))
                {
                    var matrix = qrCodeData.ModuleMatrix;
                    int moduleCount = matrix.Count;
                    if (moduleCount == 0) return;

                    double moduleSize = sizePt / moduleCount;

                    // Desenha o fundo branco para garantir legibilidade
                    graphics.DrawRectangle(XBrushes.White, xPt, yPt, sizePt, sizePt);

                    // Desenha cada célula preta como um pequeno retângulo vetorizado
                    for (int r = 0; r < moduleCount; r++)
                    {
                        var row = matrix[r];
                        for (int c = 0; c < row.Length; c++)
                        {
                            if (row[c])
                            {
                                double cellX = xPt + c * moduleSize;
                                double cellY = yPt + r * moduleSize;
                                
                                // Adiciona uma pequena folga (0.01 pt) para evitar linhas de renderização entre blocos adjacentes
                                graphics.DrawRectangle(XBrushes.Black, cellX - 0.01, cellY - 0.01, moduleSize + 0.02, moduleSize + 0.02);
                            }
                        }
                    }
                }
            }
        }
    }
}
