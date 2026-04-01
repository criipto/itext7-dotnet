using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Form.Element;
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
        private readonly List<SignerProperties> _signerProperties = new List<SignerProperties>();

        private readonly Dictionary<string, PdfSignatureFormField> _detachedSignatureFormFields =
            new Dictionary<string, PdfSignatureFormField>();

        /// used for being able to reconstruct the predictable signature name
        public const string SubFieldName = "Appearances";

        public MultipleAppearancesPdfSigner(PdfReader reader, Stream pdfStream, StampingProperties properties)
            : base(reader, pdfStream, properties)
        {
        }

        public override PdfSigner SetSignerProperties(SignerProperties properties)
        {
            _signerProperties.Add(properties);

            base.SetSignerProperties(properties);

            return this;
        }

        // We need to override the `GetSignatureField` as it exhibits side-effects (as a get-method...).
        // In the base method, it adds the signature field to the acro forms, which means that every `SignerProperties`
        // added will have its own signature instance - and that is not something that works as one would have hoped!
        // It is assumed that `SetSignerProperties` are used _before_ `GetSignatureField` as we otherwise would get a
        // default `SignerProperties` (created by base ctor), which is unwanted.
        public override PdfSignatureFormField GetSignatureField()
        {
            var fieldName = GetFieldName();

            if (_detachedSignatureFormFields.TryGetValue(fieldName, out var field))
                return field;

            var detachedSigField = new SignatureFormFieldBuilder(document, fieldName)
                .CreateSignature();

            _detachedSignatureFormFields.Add(fieldName, detachedSigField);

            return detachedSigField;
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
            foreach (var sp in _signerProperties)
            {
                var backgroundLayer = _detachedSignatureFormFields[sp.GetFieldName()].GetBackgroundLayer();

                var page = document.GetPage(sp.GetPageNumber());
                var rectangle = sp.GetPageRect();
                var widget = CreateAppearanceWidget(page, rectangle, backgroundLayer);
                subSigField.AddKid(widget);
            }

            return sigField.GetSigFieldLockDictionary();
        }

        private static PdfWidgetAnnotation CreateAppearanceWidget(PdfPage page, Rectangle rectangle, PdfFormXObject n0)
        {
            var ap = new PdfDictionary();
            ap.Put(PdfName.N, n0.GetPdfObject());

            var widget = new PdfWidgetAnnotation(rectangle);
            widget.SetFlags(PdfAnnotation.PRINT | PdfAnnotation.LOCKED);

            widget.SetPage(page);
            widget.Put(PdfName.AP, ap);

            page.AddAnnotation(widget);

            return widget;
        }
    }
}
