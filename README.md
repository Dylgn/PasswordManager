# Password Manager
A password manager that encrypts your passwords using AES.

# Admin Privileges
Admin privileges are needed to create/edit files in the Program Files (x86) folder. The password files used in the program are stored in the PasswordManager folder,
which is created when you open the program for the first time. The program asks for admin privileges when you open it, but if you somewhow get in without admin privileges,
the program will not encrypt/decrypt your files properly.

# Encryption
When you create a profile for the first time, you are given a key. This key is very important since it can't be recovered if you lose it. For the encryption to work, it
also uses an "IV" (Initalization Vector) which is written to the beginning of the file. Don't try to edit the file when its encrypted, since the program relies heavily
on the byte size of the file and where everything is located. 
* It is recommended keeping a backup copy of this file just in case anything goes wrong.
