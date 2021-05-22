﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager
{
    public partial class frmMain : Form
    {
        String path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToString();
        String fileName = "";
        String key = "";

        private enum Buttons
        {
            SignedOut = 1,
            Login,
            SignedIn
        }
        public frmMain()
        {
            InitializeComponent();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            System.IO.Directory.CreateDirectory(path + "/PasswordManager");
            path = path + "/PasswordManager/";
        }
        /*private void tempEncrypt()
        {
            // Creates file with "Test String" inside
            //byte[] test = Encoding.ASCII.GetBytes("Test String");
            var file = File.Create("test.txt");
            //file.Write(test, 0, test.Length);
            file.Close();

            // Saves key and IV for decryption
            var temp = new AesCryptoServiceProvider();
            Console.WriteLine(System.Convert.ToBase64String(temp.Key));
            Console.WriteLine(System.Convert.ToBase64String(temp.IV));

            Encryptor.Encrypt(file.Name, temp.Key, temp.IV);
        }

        private void tempDecrypt()
        {
            // Opens encrypted file
            var file = File.OpenRead("test.txt");
            var temp = new AesCryptoServiceProvider();
            temp.Key = System.Convert.FromBase64String("Vj/FQKRh0A0JtjDz/X4AMXv5I8aD1WPRQDrHWV1UFEQ=");
            //temp.IV = System.Convert.FromBase64String("oNXHQw1JSzMQ9pkvYIr8zQ==");
            var tempIV = new byte[temp.IV.Length];
            file.Read(tempIV, 0, tempIV.Length);
            file.Close();
            Encryptor.Decrypt(file.Name, temp.Key, tempIV);
        }*/
        private void btnSignIn_Click(object sender, EventArgs e)
        {
            SignInObjects(Buttons.Login);
        }
        private void btnSignUp_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to create a new account?", "Create Profile", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Choose a username between 1-16 characters.\nBlank space at the ends of your username will be ignored.", "Create Profile", "", this.Left + (this.Width / 2), this.Top + (this.Width / 2));
                if (input.Trim().Length >= 1 && input.Trim().Length <= 16)
                {
                    input = input.Trim();
                    if (!ContainsIllegalCharacters(input))
                    {
                        if (!File.Exists(path + input + ".pm"))
                        {
                            using (var provider = new AesCryptoServiceProvider())
                            {
                                // Cretes file with name and closes it
                                using (var file = File.Create(path + input + ".pm"))
                                {
                                    // Writes IV to file
                                    file.Write(provider.IV, 0, provider.IV.Length);
                                }
                                // Encrypts file
                                Encryptor.Encrypt(path + input + ".pm", provider.Key, provider.IV);

                                // Tells user to save key
                                MessageBox.Show("Use the following string to access your passwords in the future:\n\n" + System.Convert.ToBase64String(provider.Key) + "\n\nThis Key has been copied to your clipboard.\nDon't lose it, or you will lose your passwords forever!", "Key", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Clipboard.SetText(System.Convert.ToBase64String(provider.Key));
                            }
                        }
                        else
                        {
                            MessageBox.Show("This username is taken!", "Create Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } else
                    {
                        MessageBox.Show("Your username can't contain the following characters:\n / " + @"\" + ": * ? \" < > |", "Create Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else
                {
                    MessageBox.Show("Your username must be between 1-16 characters!", "Create Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }   
            }
        }
        private void btnEnter_Click(object sender, EventArgs e)
        {
            fileName = txtUser.Text.Trim() + ".pm";
            if (File.Exists(path + txtUser.Text.Trim() + ".pm"))
            {
                if (!txtUser.Text.Trim().Equals("") && !txtKey.Text.Equals(""))
                {
                    // Gets key from the user
                    byte[] byteKey = System.Convert.FromBase64String(txtKey.Text);
                    key = txtKey.Text;

                    // Gets IV from file and closes it
                    byte[] IV = GetIV(path + fileName);
                    try
                    {
                        // Decrypts file
                        Encryptor.Decrypt(path + fileName, byteKey, IV);

                        SignInObjects(Buttons.SignedIn);
                        ListCombos();
                    }
                    catch (Encryptor.IncorrectKeyException)
                    {
                        // Error if wrong key is entered
                        MessageBox.Show("The wrong key was entered.", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else
                {
                    MessageBox.Show("Either the username or key is missing.", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else
            {
                MessageBox.Show("This user does not exist!", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSignOut_Click(object sender, EventArgs e)
        {
            //BindingSource comboSource = new BindingSource();
            //comboSource.DataSource = grdCombos.DataSource;
            //List<Combo> combos = (List<Combo>)comboSource.DataSource;
            // Gets the combos from the data grid and writes them to the file
            List<Combo> combos = GetCombosFromGrid();
            byte[] IV = GetIV(path + fileName);
            using (FileStream file = File.OpenWrite(path + fileName))
            {
                // Skips IV
                file.Position = IV.Length;
                foreach (Combo combo in combos)
                {
                    // Writes each combo to file
                    file.Write(Encoding.ASCII.GetBytes(combo.Description), 0, 64);
                    file.Write(Encoding.ASCII.GetBytes(combo.Password), 0, 256);
                }
            }
            // Encrypts file
            Encryptor.Encrypt(path + fileName, System.Convert.FromBase64String(key), IV);

            // Signs out
            key = "";
            fileName = "";
            SignInObjects(Buttons.SignedOut);
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            txtUser.Text = "";
            txtKey.Text = "";
            SignInObjects(Buttons.SignedOut);
        }
        private List<Combo> GetCombosFromGrid()
        {
            List<Combo> combos = new List<Combo>();
            // Gets all the description/password combos in the grid
            foreach (DataGridViewRow row in grdCombos.Rows)
            {
                combos.Add(new Combo((string)row.Cells[0].Value, (string) row.Cells[1].Value));
            }
            return combos;
        }
        private void ListCombos()
        {
            // List combos in the data grid
            BindingSource combos = new BindingSource();
            combos.DataSource = ReadCombos();
            grdCombos.DataSource = combos;
        }
        private List<Combo> ReadCombos()
        {
            List<Combo> combos = new List<Combo>();
            using (FileStream file = File.OpenRead(path + fileName))
            {
                // Skips IV at start of file
                file.Position = 16;
                while (file.Position < file.Length)
                {
                    // Get the description and password from file
                    byte[] description = GetFromFile(file, 64);
                    byte[] password = GetFromFile(file, 256);

                    // Add description/password combo to the list
                    combos.Add(new Combo(Encoding.ASCII.GetString(description), Encoding.ASCII.GetString(password)));
                }
            }
            return combos;
        }
        private byte[] GetFromFile(FileStream file, int length)
        {
            // Gets data from the file
            byte[] array = new byte[length];
            file.Read(array, 0, array.Length);
            return array;
            /*using (MemoryStream ms = new MemoryStream())
            {
                // Gets description from the file
                byte[] array = new byte[length];
                file.Read(array, (int)file.Position, array.Length);

                // Writes the description to the stream and sets length
                ms.Write(array, 0, array.Length);
                ms.SetLength(length);

                return ms.ToArray();
            }*/
        }
        private void SignInObjects(Buttons a)
        {
            grdCombos.Enabled = false;
            grpSign.Visible = false;
            btnSignIn.Visible = false;
            btnSignUp.Visible = false;
            btnSignOut.Visible = false;
            lblUser.Visible = false;
            lblKey.Visible = false;
            txtUser.Visible = false;
            txtKey.Visible = false;
            btnEnter.Visible = false;
            btnCancel.Visible = false;
            // Only make certain objects visible
            switch (a)
            {
                case Buttons.SignedOut:
                    btnSignIn.Visible = true;
                    btnSignUp.Visible = true;
                    txtKey.Text = "";
                    txtKey.Text = "";
                    break;
                case Buttons.Login:
                    grpSign.Visible = true;
                    lblUser.Visible = true;
                    lblKey.Visible = true;
                    txtUser.Visible = true;
                    txtKey.Visible = true;
                    btnEnter.Visible = true;
                    btnCancel.Visible = true;
                    break;
                case Buttons.SignedIn:
                    grdCombos.Enabled = true;
                    grpSign.Visible = false;
                    btnSignOut.Visible = true;
                    txtKey.Text = "";
                    txtKey.Text = "";
                    break;
            }
        }
        private byte[] GetIV(string filePath)
        {
            using (var provider = new AesCryptoServiceProvider())
            using (FileStream file = File.OpenRead(path + fileName))
            {
                // Gets IV from file and closes it
                byte[] IV = new byte[provider.IV.Length];
                file.Read(IV, 0, IV.Length);
                file.Close();
                return IV;
            }
        }
        private bool ContainsIllegalCharacters(String s)
        {
            // Returns true if you're using characters unsuable in file names
            string[] chars = new string[] { "/", @"\", ":", "*", "?", "\"", "<", ">", "|" };
            foreach (string character in chars)
            {
                if (s.Contains(character))
                {
                    return true;
                }
            }
            return false;
        }
    }
}