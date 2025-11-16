using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Properties;
using iText.Layout.Properties.Grid;

namespace iText.Signatures
{
    public class MultipleAppearancesPdfSigner : PdfSigner
    {
        private readonly Dictionary<int, PdfSignatureAppearance> _appearances = new Dictionary<int, PdfSignatureAppearance>();

        private PdfSignatureAppearance _currentAppearance;

        /// used for being able to reconstruct the predictable signature name
        public const string SubFieldName = "Appearances";

        public MultipleAppearancesPdfSigner(PdfReader reader, Stream outputStream, StampingProperties properties)
            : base(reader, outputStream, properties)
        {
        }

        public MultipleAppearancesPdfSigner(PdfReader reader, Stream outputStream, String path, StampingProperties stampingProperties, SignerProperties signerProperties)
            : base(reader, outputStream, path, stampingProperties, signerProperties)
        {
        }

        [Obsolete]
        // TODO: remove when new `GetSignatureField` is adopted.
        public override PdfSignatureAppearance GetSignatureAppearance()
        {
            return _currentAppearance;
        }

        public override PdfSigner SetPageNumber(int pageNumber)
        {
            if (_appearances.TryGetValue(pageNumber, out var existing))
            {
                _currentAppearance = existing;
            }
            else
            {
                var appearance = new PdfSignatureAppearance(document, new Rectangle(0, 0), pageNumber);
                appearance.SetSignDate(signDate);

                _appearances.Add(pageNumber, appearance);

                _currentAppearance = appearance;
            }

            return this;
        }

        public override PdfSigner SetPageRect(Rectangle pageRect)
        {
            _currentAppearance.SetPageRect(pageRect);
            return this;
        }

        [Obsolete("Breaks in next major version")]
        // https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/signatures/appearance/SignatureAppearanceLayersExample.cs
        // we might have to change around to GetSignatureField and operate on that - the `CreateSubField` should then accept the signature field and use that for `N`.
        public override PdfFormXObject GetBackgroundLayer()
        {
            return _currentAppearance.GetLayer0();
        }

        // based on https://stackoverflow.com/a/78633934
        // https://github.com/mkl-public/testarea-itext7/blob/32c39692c9ac69aeee9c9abd60693c6feac8d8f9/src/test/java/mkl/testarea/itext7/signature/SignMultipleAppearances.java
        //
        // NB: some changes made compared to the mentioned work (MkII):
        //   The mentioned work constructs multiple child sigfields, however, upon inspecting the PDF structure,
        //   we noted that only the first child sigfield contained references to every added widget.
        //
        //   Upon testing we found that:
        //   * The parent sigfield _cannot_ contain all widgets - attempting to do so yields _no_ signatures.
        //   * A *single* child sigfield is created, which _can_ contain all widgets
        //     so rather than creating multiple child sigfield, we rely on a single child.
        //   * The execution order is important! If you switch around some lines thinking "what could happen" .. "EVERYTHING CAN HAPPEN!"
        protected internal override PdfSigFieldLock CreateNewSignatureFormField(PdfAcroForm acroForm, string name)
        {
            // construct parent sigfield
            var sigField = new SignatureFormFieldBuilder(document, name).CreateSignature();
            sigField.SetFieldName(name);

            // adds the parent sigfield to the form
            acroForm.AddField(sigField, null);

            if (acroForm.GetPdfObject().IsIndirect())
            {
                acroForm.SetModified();
            }
            else
            {
                document.GetCatalog().SetModified();
            }

            // remove the FT property, essentially "emptying" the parent sigfield
            sigField.Remove(PdfName.FT);

            // construct the child(/sub) sigfield to be used for the multiple widgets.
            // _NB:_ this section is different from the github example, however, we noted that
            // the first child sigfield contained references to every widget
            var subSigField = new SignatureFormFieldBuilder(document, SubFieldName).CreateSignature();
            subSigField.SetFieldName(SubFieldName); // creates a nested signature name separated by `.`

            // adds the child sigfield to the parent
            sigField.AddKid(subSigField);

            // assign the signature dictionary to the child sigfield
            var signatureDictionary = cryptoDictionary.GetPdfObject();
            subSigField.Put(PdfName.V, signatureDictionary);

            // for each defined appearance, add the newly constructed widget to the subfield
            foreach (var kv in _appearances)
            {
                var page = document.GetPage(kv.Key);
                var appearance = kv.Value;
                var widget = CreateAppearanceWidget(page, appearance);
                subSigField.AddKid(widget);
            }

            return sigField.GetSigFieldLockDictionary();
        }

        private static PdfWidgetAnnotation CreateAppearanceWidget(PdfPage page, PdfSignatureAppearance appearance)
        {
            var ap = new PdfDictionary();
            ap.Put(PdfName.N, appearance.GetLayer0().GetPdfObject());

            var rectangle = appearance.GetPageRect();
            var widget = new PdfWidgetAnnotation(rectangle);
            widget.SetFlags(PdfAnnotation.PRINT | PdfAnnotation.LOCKED);

            widget.SetPage(page);
            widget.Put(PdfName.AP, ap);

            page.AddAnnotation(widget);

            return widget;
        }
    }
}
