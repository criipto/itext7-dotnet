/*
This file is part of the iText (R) project.
Copyright (c) 1998-2024 Apryse Group NV
Authors: Apryse Software.

This program is offered under a commercial and under the AGPL license.
For commercial licensing, contact us at https://itextpdf.com/sales.  For AGPL licensing, see below.

AGPL licensing:
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using iText.Forms.Fields.Properties;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;

namespace iText.Forms.Fields {
    /// <summary>Builder for checkbox form field.</summary>
    public class CheckBoxFormFieldBuilder : TerminalFormFieldBuilder<iText.Forms.Fields.CheckBoxFormFieldBuilder
        > {
        private CheckBoxType checkType = CheckBoxType.CROSS;

        /// <summary>
        /// Creates builder for
        /// <see cref="PdfButtonFormField"/>
        /// creation.
        /// </summary>
        /// <param name="document">document to be used for form field creation</param>
        /// <param name="formFieldName">name of the form field</param>
        public CheckBoxFormFieldBuilder(PdfDocument document, String formFieldName)
            : base(document, formFieldName) {
        }

        /// <summary>Gets check type for checkbox form field.</summary>
        /// <returns>check type to be set for checkbox form field</returns>
        public virtual CheckBoxType GetCheckType() {
            return checkType;
        }

        /// <summary>Sets check type for checkbox form field.</summary>
        /// <remarks>
        /// Sets check type for checkbox form field. Default value is
        /// <see cref="iText.Forms.Fields.Properties.CheckBoxType.CROSS"/>.
        /// </remarks>
        /// <param name="checkType">check type to be set for checkbox form field</param>
        /// <returns>this builder</returns>
        public virtual iText.Forms.Fields.CheckBoxFormFieldBuilder SetCheckType(CheckBoxType checkType) {
            this.checkType = checkType;
            return this;
        }

        /// <summary>Creates checkbox form field based on provided parameters.</summary>
        /// <returns>
        /// new
        /// <see cref="PdfButtonFormField"/>
        /// instance
        /// </returns>
        public virtual PdfButtonFormField CreateCheckBox() {
            PdfButtonFormField check;
            if (GetWidgetRectangle() == null) {
                check = PdfFormCreator.CreateButtonFormField(GetDocument());
            }
            else {
                PdfWidgetAnnotation annotation = new PdfWidgetAnnotation(GetWidgetRectangle());
                annotation.SetAppearanceState(new PdfName(PdfFormAnnotation.OFF_STATE_VALUE));
                if (GetConformanceLevel() != null) {
                    annotation.SetFlag(PdfAnnotation.PRINT);
                }
                check = PdfFormCreator.CreateButtonFormField(annotation, GetDocument());
            }
            check.DisableFieldRegeneration();
            check.pdfConformanceLevel = GetConformanceLevel();
            check.SetCheckType(checkType);
            check.SetFieldName(GetFormFieldName());
            // the default behavior is to automatically calculate the fontsize
            check.SetFontSize(0);
            check.Put(PdfName.V, new PdfName(PdfFormAnnotation.OFF_STATE_VALUE));
            if (GetWidgetRectangle() != null) {
                SetPageToField(check);
            }
            check.EnableFieldRegeneration();
            return check;
        }

        /// <summary><inheritDoc/></summary>
        protected internal override iText.Forms.Fields.CheckBoxFormFieldBuilder GetThis() {
            return this;
        }
    }
}
