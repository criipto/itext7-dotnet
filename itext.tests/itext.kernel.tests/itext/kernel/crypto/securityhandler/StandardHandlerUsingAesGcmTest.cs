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
using System.Collections.Generic;
using iText.Bouncycastleconnector;
using iText.Commons.Bouncycastle;
using iText.Kernel.Logs;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;
using iText.Test.Attributes;

namespace iText.Kernel.Crypto.Securityhandler {
    [NUnit.Framework.Category("BouncyCastleIntegrationTest")]
    public class StandardHandlerUsingAesGcmTest : ExtendedITextTest {
        private static readonly IBouncyCastleFactory FACTORY = BouncyCastleFactoryCreator.GetFactory();

        public static readonly String SRC = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/kernel/crypto/securityhandler/StandardHandlerUsingAesGcmTest/";

        public static readonly String DEST = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/kernel/crypto/securityhandler/StandardHandlerUsingAesGcmTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void SetUp() {
            CreateOrClearDestinationFolder(DEST);
        }

        [NUnit.Framework.Test]
        [LogMessage(VersionConforming.NOT_SUPPORTED_AES_GCM, Ignore = true)]
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        public virtual void TestSimpleEncryptDecryptTest() {
            String srcFile = SRC + "simpleDocument.pdf";
            String cmpFile = SRC + "cmp_simpleDocument.pdf";
            String outFile = DEST + "simpleEncryptDecryptTest.pdf";
            DoEncrypt(srcFile, outFile);
            TryCompare(outFile, cmpFile);
        }

        [NUnit.Framework.Test]
        [LogMessage(VersionConforming.NOT_SUPPORTED_AES_GCM, Ignore = true)]
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        public virtual void TestSimpleEncryptDecryptPdf15Test() {
            String srcFile = SRC + "simpleDocument.pdf";
            String cmpFile = SRC + "cmp_simpleDocument.pdf";
            String outFile = DEST + "notSupportedVersionDocument.pdf";
            byte[] userBytes = "secret".GetBytes(System.Text.Encoding.UTF8);
            byte[] ownerBytes = "supersecret".GetBytes(System.Text.Encoding.UTF8);
            int perms = EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_DEGRADED_PRINTING;
            WriterProperties wProps = new WriterProperties().SetStandardEncryption(userBytes, ownerBytes, perms, EncryptionConstants
                .ENCRYPTION_AES_GCM);
            PdfDocument ignored = new PdfDocument(new PdfReader(srcFile), new PdfWriter(outFile, wProps));
            ignored.Close();
            TryCompare(outFile, cmpFile);
        }

        [NUnit.Framework.Test]
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        public virtual void TestKnownOutput() {
            String srcFile = SRC + "encryptedDocument.pdf";
            String cmpFile = SRC + "simpleDocument.pdf";
            TryCompare(srcFile, cmpFile);
        }

        // In all these tampered files, the stream content of object 14 has been modified.
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        [NUnit.Framework.Test]
        public virtual void TestMacTampered() {
            String srcFile = SRC + "encryptedDocumentTamperedMac.pdf";
            AssertTampered(srcFile);
        }

        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        [NUnit.Framework.Test]
        public virtual void TestIVTampered() {
            String srcFile = SRC + "encryptedDocumentTamperedIv.pdf";
            AssertTampered(srcFile);
        }

        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        [NUnit.Framework.Test]
        public virtual void TestCiphertextTampered() {
            String srcFile = SRC + "encryptedDocumentTamperedCiphertext.pdf";
            AssertTampered(srcFile);
        }

        [NUnit.Framework.Test]
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        [LogMessage(iText.IO.Logs.IoLogMessageConstant.ENCRYPTION_ENTRIES_P_AND_ENCRYPT_METADATA_NOT_CORRESPOND_PERMS_ENTRY
            )]
        public virtual void PdfEncryptionWithEmbeddedFilesTest() {
            byte[] documentId = new byte[] { (byte)88, (byte)189, (byte)192, (byte)48, (byte)240, (byte)200, (byte)87, 
                (byte)183, (byte)244, (byte)119, (byte)224, (byte)109, (byte)226, (byte)173, (byte)32, (byte)90 };
            byte[] password = new byte[] { (byte)115, (byte)101, (byte)99, (byte)114, (byte)101, (byte)116 };
            Dictionary<PdfName, PdfObject> encMap = new Dictionary<PdfName, PdfObject>();
            encMap.Put(PdfName.R, new PdfNumber(7));
            encMap.Put(PdfName.V, new PdfNumber(6));
            encMap.Put(PdfName.P, new PdfNumber(-1852));
            encMap.Put(PdfName.EFF, PdfName.FlateDecode);
            encMap.Put(PdfName.StmF, PdfName.Identity);
            encMap.Put(PdfName.StrF, PdfName.Identity);
            PdfDictionary embeddedFilesDict = new PdfDictionary();
            embeddedFilesDict.Put(PdfName.FlateDecode, new PdfDictionary());
            encMap.Put(PdfName.CF, embeddedFilesDict);
            encMap.Put(PdfName.EncryptMetadata, PdfBoolean.FALSE);
            encMap.Put(PdfName.O, new PdfString("\u0006¡Ê\u009A<@\u009DÔG\u0013&\u008C5r\u0096\u0081i!\u0091\u000Fªìh=±\u0091\u0006Að¨\u008D\"¼\u0018?õ\u001DNó»{y\u0091)\u0090vâý"
                ));
            encMap.Put(PdfName.U, new PdfString("ôY\u009DÃ\u0017Ý·Ü\u0097vØ\fJ\u0099c\u0004áÝ¹ÔB\u0084·9÷\u008F\u009D-¿xnkþ\u0086Æ\u0088º\u0086ÜTÿëÕï\u0018\u009D\u0016-"
                ));
            encMap.Put(PdfName.OE, new PdfString("5Ë\u009EUÔº\u0007 Nøß\u0094ä\u001DÄ_wnù\u001AKò-\u007F\u00ADQ²Ø \u001FSJ"
                ));
            encMap.Put(PdfName.UE, new PdfString("\u000B:\rÆ\u0004\u0094Ûìkþ,ôBS9ü\u001E³\u0088\u001D(\u0098ºÀ\u0010½\u0082.'`kñ"
                ));
            encMap.Put(PdfName.Perms, new PdfString("\u008F»\u0080.òç\u0011\u001Et\u0012\u00905\u001B\u0019\u0014«"));
            PdfDictionary dictionary = new PdfDictionary(encMap);
            PdfEncryption encryption = new PdfEncryption(dictionary, password, documentId);
            NUnit.Framework.Assert.IsTrue(encryption.IsEmbeddedFilesOnly());
        }

        [NUnit.Framework.Test]
        [LogMessage(KernelLogMessageConstant.MD5_IS_NOT_FIPS_COMPLIANT, Ignore = true)]
        public virtual void PdfEncryptionWithMetadataTest() {
            byte[] documentId = new byte[] { (byte)88, (byte)189, (byte)192, (byte)48, (byte)240, (byte)200, (byte)87, 
                (byte)183, (byte)244, (byte)119, (byte)224, (byte)109, (byte)226, (byte)173, (byte)32, (byte)90 };
            byte[] password = new byte[] { (byte)115, (byte)101, (byte)99, (byte)114, (byte)101, (byte)116 };
            Dictionary<PdfName, PdfObject> encMap = new Dictionary<PdfName, PdfObject>();
            encMap.Put(PdfName.R, new PdfNumber(7));
            encMap.Put(PdfName.V, new PdfNumber(6));
            encMap.Put(PdfName.P, new PdfNumber(-1852));
            encMap.Put(PdfName.StmF, PdfName.StdCF);
            encMap.Put(PdfName.StrF, PdfName.StdCF);
            PdfDictionary embeddedFilesDict = new PdfDictionary();
            embeddedFilesDict.Put(PdfName.FlateDecode, new PdfDictionary());
            encMap.Put(PdfName.CF, embeddedFilesDict);
            encMap.Put(PdfName.EncryptMetadata, PdfBoolean.TRUE);
            encMap.Put(PdfName.O, new PdfString("\u0006¡Ê\u009A<@\u009DÔG\u0013&\u008C5r\u0096\u0081i!\u0091\u000Fªìh=±\u0091\u0006Að¨\u008D\"¼\u0018?õ\u001DNó»{y\u0091)\u0090vâý"
                ));
            encMap.Put(PdfName.U, new PdfString("ôY\u009DÃ\u0017Ý·Ü\u0097vØ\fJ\u0099c\u0004áÝ¹ÔB\u0084·9÷\u008F\u009D-¿xnkþ\u0086Æ\u0088º\u0086ÜTÿëÕï\u0018\u009D\u0016-"
                ));
            encMap.Put(PdfName.OE, new PdfString("5Ë\u009EUÔº\u0007 Nøß\u0094ä\u001DÄ_wnù\u001AKò-\u007F\u00ADQ²Ø \u001FSJ"
                ));
            encMap.Put(PdfName.UE, new PdfString("\u000B:\rÆ\u0004\u0094Ûìkþ,ôBS9ü\u001E³\u0088\u001D(\u0098ºÀ\u0010½\u0082.'`kñ"
                ));
            encMap.Put(PdfName.Perms, new PdfString("\u008F»\u0080.òç\u0011\u001Et\u0012\u00905\u001B\u0019\u0014«"));
            PdfDictionary dictionary = new PdfDictionary(encMap);
            PdfEncryption encryption = new PdfEncryption(dictionary, password, documentId);
            NUnit.Framework.Assert.IsTrue(encryption.IsMetadataEncrypted());
        }

        private void DoEncrypt(String input, String output) {
            // Pick user/owner password
            byte[] userBytes = "secret".GetBytes(System.Text.Encoding.UTF8);
            byte[] ownerBytes = "supersecret".GetBytes(System.Text.Encoding.UTF8);
            // Set usage permissions
            int perms = EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_DEGRADED_PRINTING;
            WriterProperties wProps = new WriterProperties().SetPdfVersion(PdfVersion.PDF_2_0).SetStandardEncryption(userBytes
                , ownerBytes, perms, EncryptionConstants.ENCRYPTION_AES_GCM);
            // Instantiate input/output document
            using (PdfDocument docIn = new PdfDocument(new PdfReader(input))) {
                using (PdfDocument docOut = new PdfDocument(new PdfWriter(output, wProps))) {
                    // Copy one page from input to output
                    docIn.CopyPagesTo(1, 1, docOut);
                }
            }
        }

        private void TryCompare(String outPdf, String cmpPdf) {
            new CompareTool().CompareByContent(outPdf, cmpPdf, DEST, "diff", "secret".GetBytes(System.Text.Encoding.UTF8
                ), null);
        }

        private void AssertTampered(String outFile) {
            String cmpFile = SRC + "cmp_simpleDocument.pdf";
            NUnit.Framework.Assert.Catch(typeof(Exception), () => TryCompare(outFile, cmpFile));
        }
    }
}
