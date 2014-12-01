using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Lab7_KD
{
    public partial class Form1 : Form
    {
        private String key = "";
        private String path = "";
        public Form1()
        {
            InitializeComponent();

            // clears the default text for the file
            fileDialog.FileName = "";
        }
        // Take value from the fileName text box and save as string
        private void fileNameText_TextChanged(object sender, System.EventArgs e)
        {
            if (fileNameText.Text != null){this.path = fileNameText.Text;}
        }

        // Take value from the keyText text box and save as string
        private void keyText_TextChanged(object sender, System.EventArgs e)
        {
            if (keyText.Text != null){this.key = keyText.Text;}
        }
        // Open browse files option when selectFileButton is clicked
        private void selectFileButton_Click(object sender, System.EventArgs e)
        {
            // openFileDialog tool to open file choice window
            if (fileDialog.ShowDialog(this) == DialogResult.OK)
                fileNameText.Text = fileDialog.FileName;
            this.Invalidate();
        }
        // Button calls the encrypt method that I customized from the MSDN code
        private void encryptButton_Click(object sender, System.EventArgs e)
        {
            Encrypt();
            this.Invalidate();
        }
        // Button calls the decrypt method that I customized from the MSDN code
        private void decryptButton_Click(object sender, System.EventArgs e)
        {
            Decrypt();
            this.Invalidate();
        }

        // This is a method to turn the key string entered into a byte array per instructions 
        private byte[] byteArray()
        {
            // The key is formed from the password string by taking the low order 8 bits of each Unicode character and storing it in the byte array.
            byte[] kArray = Enumerable.Repeat((byte)0, 8).ToArray();
            for (int i = 0; i < key.Length; i++)
            {
                //If the string is more than 8 characters the 9th character's 8 bit value is added to the first byte in the array
                byte b = (byte)key[i];
                kArray[i % 8] = (byte)(kArray[i % 8] + b);
            }
            return kArray;
        }
        // method created to call before saving a file just to check if a files exists and needs overwrite
        public bool needsOverwrite(string outName)
        {
            if (File.Exists(outName))// built in method that tests for existing files 
            {
                var result = MessageBox.Show("Output file exists. Overwrite?",
                "Error",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
                //check users answer and return value
                if (result == DialogResult.No)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
        //Encryption Decryption code refrence: http://msdn.microsoft.com/en-us/library/system.security.cryptography.descryptoserviceprovider(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
        // Encrypt File Method taken from MSDN (customized, added exceptions) 
        private void Encrypt()
        {
            // execute the code in a try block to catch necessary exceptions.
            try
            {
                // Variables needed to encrypt from user entry
                string inName = this.path;
                string outName = this.path + ".des";

                // throw manual exception if no key is entered so the code breaks
                if (this.keyText.Text == "")
                {
                    throw new Exception("Please enter a key.");
                }
                if (!needsOverwrite(outName)) // Check to see if the file already exists and if user doesn't want to overwrite
                {
                    throw new IOException("File not overwritten.");
                    // Else continue and overwrite
                }
                // Use the method for the turining string into byteArray to get bytes 
                byte[] desKey = this.byteArray();
                byte[] desIV = this.byteArray();

                //Create the file streams to handle the input and output files.
                FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                //Create variables to help with read and write. 
                byte[] bin = new byte[100];     //This is intermediate storage for the encryption. 
                long rdlen = 0;                 //This is the total number of bytes written. 
                long totlen = fin.Length;       //This is the total length of the input file. 
                int len;                        //This is the number of bytes to be written at a time.

                DES des = new DESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(desKey, desIV), CryptoStreamMode.Write);

                Console.WriteLine("Encrypting...");

                //Read from the input file, then encrypt and write to the output file. 
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    encStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                    Console.WriteLine("{0} bytes processed", rdlen);
                }

                encStream.Close();
                fout.Close();
                fin.Close();
                Console.WriteLine("Done.");
            }
            //  If something in the try block throws an expections, catch here.
            catch (Exception e)
            {
                if (e is System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Could not open source or destination file",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                else if (e is System.Security.Cryptography.CryptographicException)
                {
                    MessageBox.Show("Bad key or file.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                else // This is the custon exception that is sent if manual exception is thrown
                {
                    MessageBox.Show(e.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
        // Encrypt File Method taken from MSDN (customized, added exceptions) 
        private void Decrypt()
        {
            try
            {
                // Variables needed to encrypt from user entry
                string inName = this.path;

                if (this.keyText.Text == "") // Check for empty key
                {
                    throw new Exception("Please enter a key.");
                }
                if (Path.GetExtension(inName) != ".des")
                {
                    throw new Exception("Not a .des file.");
                }
                string outName = Path.ChangeExtension(path, "");  // Remove ".des" extension
                if (!needsOverwrite(outName)) // Check to see if the file already exists and if user doesn't want to overwrite
                {
                    throw new IOException("File not overwritten");
                    // Else continue and overwrite
                }
                byte[] desKey = this.byteArray();
                byte[] desIV = this.byteArray();

                //Create the file streams to handle the input and output files.
                FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                //Create variables to help with read and write. 
                byte[] bin = new byte[100];     //This is intermediate storage for the encryption. 
                long rdlen = 0;                 //This is the total number of bytes written. 
                long totlen = fin.Length;       //This is the total length of the input file. 
                int len;                        //This is the number of bytes to be written at a time.

                DES des = new DESCryptoServiceProvider();
                CryptoStream decStream = new CryptoStream(fout, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Write);

                Console.WriteLine("Decrypting...");

                //Read from the input file, then encrypt and write to the output file. 
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    decStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                    Console.WriteLine("{0} bytes processed", rdlen);
                }
                decStream.Close();
                fout.Close();
                fin.Close();
                Console.WriteLine("Done.");
            }
            //  If something in the try block throws an expections, catch here.
            catch (Exception e)
            {
                if (e is System.Security.Cryptography.CryptographicException)
                {
                    MessageBox.Show("Bad key or file.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                else if (e is System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Could not open source or destination file",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                else // This is the custon exception that is sent if manual exception is thrown
                {
                    MessageBox.Show(e.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
    }
}
