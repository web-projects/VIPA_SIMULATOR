using FileTransfer;
using FileTransfer.Providers;
using Xunit;

namespace FileTransferTests
{
    public class FileTransferTests
    {
        readonly FileTransferProviderImpl subject;

        public FileTransferTests()
        {
            subject = new FileTransferProviderImpl();
        }

        [Fact]
        public void TransferFile_SendsFileToSFTPServer_WhenCalledWithPublicKey()
        {
            SftpConnectionParameters clientParams = new SftpConnectionParameters()
            {
                Hostname = "10.10.100.120",
                Username = "jonbianco",
                Port = 22,
                Filename = @"C:\Public\Temp\test.txt"
            };

            subject.TransferFile(clientParams);
        }

        [Fact]
        public void TransferFile_SendsFileToSFTPServer_WhenCalledWithPassword()
        {
            SftpConnectionParameters clientParams = new SftpConnectionParameters()
            {
                Hostname = "10.10.100.120",
                Username = "jonbianco",
                Password = "ilfalco1",
                Port = 22,
                Filename = @"C:\Public\Temp\test.txt"
            };

            subject.TransferFile(clientParams);
        }
    }
}