using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace Reg_reader
{
    public partial class Form1 : Form
    {
       
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region all structures
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
   struct offsets {   //subkey_list
    public int  block_size;
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] block_type;// // "lf" "il" "ri"
   
  public  ushort count;   
 //  public uint  first; //verify uint
  // public uint  hash;  //verify uint
};

   [Serializable]
   [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct key_block  { 
    public int  block_size;

    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] block_type;// "nk"

    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] dum1;
   
    public uint   subkey_count;  // not used anywhere
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dum2;

    public int   offsettosubkeys;  //stores the offset to subkey list
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] dum3;

    public int   value_count;  // number of values in the 
    public int   offsettovaluelist;
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 28)]
    public byte[] dum4;
   
    public ushort len;
    public ushort du;
    //char  name; 
};

[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct value_block {
    public int  block_size;

    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] block_type;// "vk"
   
    public ushort name_len;
    public uint  size;
    public uint  offset;
    public uint  value_type;
    public ushort flags;
    public ushort dummy;
   // char  name; 
};

[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct hbin_block
{
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
    public string hbin;  //"hbin"
    uint first_hive_bin;
    uint this_bin_size;
     [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
    public string dumm;
     uint next_hive_bin;

};


[Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct regheader
{
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] regf;

    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] dummya;

   public uint offset;  //20H = offset from root to first node - the root node.
    uint offsettolasthbin;
    uint dum;

    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] regname; //dotted byte array UTF-16

}
        #endregion
#region all global data
public Queue<TreeNode> searchqueue = new Queue<TreeNode>();
int stackthisnode = 1;
Button nb;
public uint tmpuint;
int found = 0;
public int root = 4096;
offsets off = new offsets();
key_block tmpblock = new key_block();
value_block tvalueblock = new value_block();
IntPtr intPtr;
Button button1, button2, b2,b3,b4;
TreeView treeView1, treeView2;
ImageList imglist;
Label l,l1,l2,l3,l4,l5,l6,l7,l8,l9,l10,l11; 
        int t1nk = 0, t1vk=0, t2vk=0, t2nk=0, modifiedkeys=0, deletedkeys=0, unchangedkeys=0,addedkeys=0;
public struct stackobject
{
    public int offset;
    public TreeNode treend;
}
public regheader fileregheader;
Panel pnl;
public byte[] data;
TreeNode treenode, treenode2;
TreeNode[] PlotTree = new TreeNode[2];
int final = 1, buffer = 0;

stackobject tstackobject = new stackobject();
public int treloaded = 0;
public Queue<TreeNode> treequeue = new Queue<TreeNode>();
public Queue<stackobject> jobstack = new Queue<stackobject>();
public Queue<int> listack = new Queue<int>();
PictureBox p1, p2, p3, p4;     
TreeNode presentnode;
int y1;
#endregion

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            imglist = new ImageList();
           
          //  imglist.Images.Add(Image.FromFile("folder.bmp"));
            imglist.Images.Add(Reg_reader.Properties.Resources.Folder);
            imglist.Images.Add(Reg_reader.Properties.Resources.Key);
            imglist.Images.Add(Reg_reader.Properties.Resources.str);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            pnl = new Panel();
            pnl.Location = new Point(500, 0);
            pnl.Size = new Size(200, 490);

            this.Icon = Reg_reader.Properties.Resources.Key1;
    this.ShowIcon = true;

        //    pnl.BackColor = Color.Red;
            
         
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Name = "Form1";
            this.Text = "Reg Hive Viewer/Comparator";
            this.button1 = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.treeView2 = new System.Windows.Forms.TreeView(); //only used for comparing, not plotting
            this.button2 = new System.Windows.Forms.Button();
       
            this.ResumeLayout(false);

            this.button1.Location = new System.Drawing.Point(5, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(190, 30);
            this.button1.TabIndex = 0;
            this.button1.Text = "Open First Reg Hive";
            this.button1.Click += new System.EventHandler(this.openreghive1);

            pnl.Controls.Add(button1);
            b2 = new Button();
            this.b2.Location = new System.Drawing.Point(5, 95);
            this.b2.Name = "button3";
            this.b2.Size = new System.Drawing.Size(70, 30);
            this.b2.TabIndex = 0;
            this.b2.Text = "Expand All";
            this.Controls.Add(b2);
            this.b2.Click += new System.EventHandler(this.expandtree);
            pnl.Controls.Add(b2);

            b3 = new Button();
            this.b3.Location = new System.Drawing.Point(80, 95);
            this.b3.Name = "button3";
            this.b3.Size = new System.Drawing.Size(70, 30);
            this.b3.TabIndex = 0;
            this.b3.Text = "Collapse All";
            this.Controls.Add(b3);
              this.b3.Click += new System.EventHandler(this.collapsetree);
              pnl.Controls.Add(b3);

              b4 = new Button();
              this.b4.Location = new System.Drawing.Point(155, 95);
              this.b4.Name = "button3";
              this.b4.Size = new System.Drawing.Size(38, 30);
              this.b4.TabIndex = 0;
              this.b4.Text = "Help";
              this.Controls.Add(b4);
              this.b4.Click += new System.EventHandler(this.help);
              pnl.Controls.Add(b4);

            this.button2.Location = new System.Drawing.Point(5, 50);
            this.button2.Name = "button1";
            this.button2.Size = new System.Drawing.Size(188, 30);
            this.button2.TabIndex = 0;
            this.button2.Text = "Open Second Reg Hive";
            this.button2.Enabled = false;
            this.button2.Click += new System.EventHandler(this.openreghive2);
            pnl.Controls.Add(button2);
            
            this.treeView1.LineColor = System.Drawing.Color.ForestGreen;
            this.treeView1.Location = new System.Drawing.Point(5, 5);
            this.treeView1.Name = "treeView1";
            this.treeView1.ImageList = imglist;
          //  treeView1.ImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(490, 490);
           
            int Y = 140;
            l7 = new Label();
            l7.Text = "Compare Status of Hive 2 with first.";
            l7.Size = new Size(190, 20);
            l7.Location = new System.Drawing.Point(5, Y + 145);
           // this.Controls.Add(l7);
            pnl.Controls.Add(l7);

            y1 = 310;
            p1 = new PictureBox();
            this.p1.Location = new System.Drawing.Point(5, y1);  //Picture box location
            this.p1.Name = "color box";
            this.p1.BorderStyle = BorderStyle.Fixed3D;
            this.p1.Size = new System.Drawing.Size(30, 15);  //Picture box size
            this.p1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.p1.BackColor = Color.Red;
           // this.Controls.Add(p1);
            pnl.Controls.Add(p1);

            l8 = new Label();
            l8.Text = "Nodes Deleted       : " + deletedkeys.ToString();
            l8.Size = new Size(200, 20);
            l8.Location = new System.Drawing.Point(35, y1);
          //  this.Controls.Add(l8);
            pnl.Controls.Add(l8);
            
            p2 = new PictureBox();
            this.p2.Location = new System.Drawing.Point(5, y1+30);  //Picture box location
            this.p2.Name = "color box";
            this.p2.BorderStyle = BorderStyle.Fixed3D;
            this.p2.Size = new System.Drawing.Size(30, 15);  //Picture box size
            this.p2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.p2.BackColor = Color.Green;
          //  this.Controls.Add(p2);
            pnl.Controls.Add(p2);

            l9 = new Label();
            l9.Text = "Nodes Unchanged : " + unchangedkeys.ToString();
            l9.Size = new Size(200, 20);
            l9.Location = new System.Drawing.Point(35, y1+30);
          //  this.Controls.Add(l9);
            pnl.Controls.Add(l9);

            p3 = new PictureBox();
            this.p3.Location = new System.Drawing.Point(5, y1+60);  //Picture box location
            this.p3.Name = "color box";
            this.p3.BorderStyle = BorderStyle.Fixed3D;
            this.p3.Size = new System.Drawing.Size(30, 15);  //Picture box size
            this.p3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.p3.BackColor = Color.Pink;
            pnl.Controls.Add(p3);
          //  pnl.Controls.Add(p3);
            l10 = new Label();
            l10.Text = "Nodes Modified      : " + modifiedkeys.ToString();
            l10.Size = new Size(200, 20);
            l10.Location = new System.Drawing.Point(35, y1 + 60);
          //  this.Controls.Add(l10);
            pnl.Controls.Add(l10);


            p4 = new PictureBox();
            this.p4.Location = new System.Drawing.Point(5, y1+90);  //Picture box location
            this.p4.Name = "color box";
            this.p4.BorderStyle = BorderStyle.Fixed3D;
            this.p4.Size = new System.Drawing.Size(30, 15);  //Picture box size
            this.p4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.p4.BackColor = Color.SkyBlue;
           // this.Controls.Add(p4);
            pnl.Controls.Add(p4);
           // this.colbox.Click += new System.EventHandler(this.Showcolordialog);
         
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
           this.ClientSize = new System.Drawing.Size(700, 500);
        //    this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            l11 = new Label();
            l11.Text = "Nodes Added         : " + addedkeys.ToString();
            l11.Size = new Size(200, 20);
            l11.Location = new System.Drawing.Point(35, y1 + 90);
          //  this.Controls.Add(l11);
            pnl.Controls.Add(l11);

            l = new Label();
            l.Text = "First RegHive Status:";
            l.Size = new Size(200, 20);
            l.Location = new System.Drawing.Point(15, Y);
          //  this.Controls.Add(l);
            pnl.Controls.Add(l);

            l1 = new Label();
            l1.Text = "Node Keys:";
            l1.Size = new Size(200,20);
            l1.Location = new System.Drawing.Point(15, Y+20);
         //   this.Controls.Add(l1);
            pnl.Controls.Add(l1);

            l2 = new Label();
            l2.Text = "Value Keys:";
            l2.Size = new Size(200, 20);
            l2.Location = new System.Drawing.Point(15, Y+40);
           // this.Controls.Add(l2);
            pnl.Controls.Add(l2);

            l3 = new Label();
            l3.Text = "Second RegHive Status:";
            l3.Size = new Size(200, 20);
            l3.Location = new System.Drawing.Point(15, Y+80);
           // this.Controls.Add(l3);
            pnl.Controls.Add(l3);

            l4 = new Label();
            l4.Text = "Node Keys:";
            l4.Size = new Size(200, 20);
            l4.Location = new System.Drawing.Point(15, Y+100);
            pnl.Controls.Add(l4);

            l5 = new Label();
            l5.Text = "Value Keys:";
            l5.Size = new Size(200, 20);
            l5.Location = new System.Drawing.Point(15, Y+120);
            pnl.Controls.Add(l5);

            l6 = new Label();
            l6.Text = "STATUS:";
            l6.Size = new Size(190, 30);
            l6.Location = new System.Drawing.Point(5, Y+285);
            pnl.Controls.Add(l6);

            this.Resize += new EventHandler(ResizeForm_Resize);
            this.MinimumSize = new Size(700,530);
            this.Controls.Add(pnl);
            this.Controls.Add(this.treeView1);
         
            
            this.ResumeLayout(false);


        }

        private void ResizeForm_Resize(object sender, System.EventArgs e)
        {
         int   X = this.ClientSize.Width;
          int  Y = this.ClientSize.Height;
          treeView1.Size = new Size(X - 210, Y - 10);
          treeView1.Location = new Point(5, 5);
          pnl.Location = new Point(X - 200, 0);
    

        
        }
        private void help(object sender, EventArgs e)
        {
            MessageBox.Show("This program can be used to view Windows registry hive files and can also compare the two files. Comparing would normally be done on the same file after an operation was carried out to see the changes made by a software to the registry. For comparing, color coding defines the results.\n\n\n 1. Green means those nodes were not changed. \n 2. Red means these nodes were deleted. These nodes were present in the first hive but not in the second.\n 3. SkyBlue means these nodes were added by software. These are the nodes present in second hive but not in the first.\n 4. Pink means the data was changed for value keys. \n\n\nSTATUS label at the bottom shows the current software state. Loading some hives can take more time and it may feel that the software has hung. This label should be checked to see if software is busy. Code would normally crash if file has an issue. If it doesn't crash, just relax, it will show up.", "  Using the Software");
        }
       
        private void expandtree(object sender, EventArgs e)
        {
            if (treenode != null) treeView1.ExpandAll();
        }
        private void collapsetree(object sender, EventArgs e)
        {
            if (treenode != null) treeView1.CollapseAll();
        }
        private void openreghive1(object sender, EventArgs e)
    {
            
            OpenFileDialog fileOpen = new OpenFileDialog();
            t1nk = 0; t1vk = 0; t2vk = 0; t2nk = 0; modifiedkeys = 0;
            fileOpen.InitialDirectory = ".\\";
            fileOpen.Filter = "All file (*.*)| *.*";
            fileOpen.FilterIndex = 1;
            fileOpen.ShowHelp = true;
          //  l.Text = "Total Number\n of key = 0";
            fileOpen.RestoreDirectory = false; //true;
            if (fileOpen.ShowDialog() == DialogResult.Cancel) return;
            string file = fileOpen.FileName; FileStream fs = null; ;
            try
            {
                 fs = File.OpenRead(file);
            }
            catch
            {
                MessageBox.Show("Can not open file. In use."); return;
            }
            this.Refresh();
            this.SuspendLayout();
        data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            if (fs.Length < 4096) { MessageBox.Show("Not a reg hive file."); fs.Close(); return; }
            fs.Close();
            if (data[0] != 'r' || data[1] != 'e' || data[2] != 'g' || data[3] != 'f') { MessageBox.Show("Not a reg hive file."); return; }
            if (treloaded != 0) { treeView1.Nodes.Clear(); treenode.Nodes.Clear(); treloaded = 1; }
            l6.BackColor = Color.Red;
            l6.Text = "Status: Reading Hive one.";
            l6.Refresh();
            this.button2.Enabled = false;
// start by reading the registry header (only reads the file regname and offset: not used)
        string regn =readreghead();
        int presentkey = root + (int)fileregheader.offset;  //,4128
           int start=0 ;
        string tstring = "", vstring="";
        
// add regname to the treeview
          treenode = new TreeNode();
        //treenode.Text = ""+ regn;// +regname;
          treenode.Text = "--REGISTRY--ROOT--";
          treeView1.Nodes.Add(treenode);
          treloaded = 1;
      
//start the string search

        
      //  presentnode = new TreeNode();  //this only initializes the present node with default values.
                                            

     
            //add first job to stack
        tstackobject.offset = 4128;
        tstackobject.treend = treenode;
         jobstack.Enqueue(tstackobject);
                  
        presentnode = treenode;   // present node is just a pointer to treenode. Its not a separate copy of it.
    
        int tcount = 0, taddress = 0, ad = 0, iindex = 2, lf = 0, hash = 4;
  
        uint hex = 0x80000000;
            while(jobstack.Count!=0)
            {
               
                tstackobject = jobstack.Dequeue();// .Pop();
                presentkey = tstackobject.offset;
                presentnode = tstackobject.treend;
                readkeyblock(presentkey);  // read keyblock in tmpblock
               tstring = getstringataddress(presentkey + 80 , tmpblock.len); //read the keyblock name. not necessary
                TreeNode newnode = new TreeNode();
                newnode.Text = tstring;
                if (start == 0) { start = 1; presentnode = treenode; }  //avoid the first root hive name

                else
                {
                    //newnode.p
                    presentnode.Nodes.Add(newnode);
                    presentnode = newnode;
                    
                }
                t1vk += tmpblock.value_count;

                //read all value keys in the tmpblock key block
                #region
                if (tmpblock.value_count != 0)
                {
                    int blankcount = 0; 
                    for(int v=0;v < tmpblock.value_count; v++){
                        // offset to value list is a series of address location (after 4 bytes of offset)
                        // these adress points to value blocks
                        ad= (int)readuintataddress(tmpblock.offsettovaluelist + root +4 + 4*v) + root;
                        readvalueblock(ad);
                        iindex = 2; //image index default to string type
                        if (tvalueblock.block_type[0] != 'v' || tvalueblock.block_type[1] != 'k') { MessageBox.Show("Value Block not VK type. Quitting."); return; }
                        if (tvalueblock.size > 0)
                        {
                            vstring = "";
                            TreeNode newnode1 = new TreeNode();
                            if (tvalueblock.name_len == 0) { blankcount++; vstring = "Default: "; } //the default key
                            else { vstring = getstringataddress(ad + Marshal.SizeOf(tvalueblock), tvalueblock.name_len) + ": "; }
                            newnode1.Tag = vstring.Length;
                            switch (tvalueblock.value_type)
                            {
                                case 1: // simple dotted string
                                case 2: // "system root" type string keys
                                case 6: //regkey link in string frmat
                                    // if data is in offset, than there can be only two characters in it as its in dotted format. if size is two, it means zero size string, if size is 4, one characted string.
                                    if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; if(tvalueblock.size <3) vstring += "BLANK";else vstring= string.Concat(vstring, (char)tvalueblock.offset); }
                                    else
                                    {
                                        vstring += getUTF16stringataddress((int)tvalueblock.offset + root + 4, (int)tvalueblock.size);
                                    }
                                    break;

                                case 7: //series of string terminated by 00 and also then fully terminated by 00
                                    if (tvalueblock.size >= hex) { vstring += "BLANK"; break; }
                                    vstring += getUTF16stringataddress((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break;
                                case 4: // Dword 4 bytes . would normally be stored in offset value
                                    iindex = 1;
                                    if (tvalueblock.size >= hex) vstring += tvalueblock.offset.ToString();
                                    else vstring += "INVALID DWORD LENGTH";
                                    break;
                                case 3: //binary data
                                case 0:
                                case 8:
                                case 9:
                                case 10:
                                case 11:  // 64bit Endian Integer...Regedit marks this as binary value
                                    iindex = 1;
                                    if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; vstring += offsettostring(tvalueblock.offset, tvalueblock.size); }
                                    else { vstring += getstringataddressvalueof3((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break; }
                                    break;

                                default: iindex = 1;
                                    if (tvalueblock.value_type < 15) MessageBox.Show("Use this file to add another value type");
                                    if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; vstring += offsettostring(tvalueblock.offset, tvalueblock.size); }
                                    else { vstring += getstringataddress((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break; }
                                    break;

                            } //switch
                            //}
                            if (blankcount > 1) { MessageBox.Show("More than one blank item in node."); break; }

                            newnode1.Text = vstring;

                            newnode1.ImageIndex = iindex;
                            newnode1.SelectedImageIndex = iindex;
                            presentnode.Nodes.Add(newnode1);
                        }
                        
                }
                }
                #endregion

                // read all subkeys in the tmpblock and add them to the stack
                //tmpblock.offsettosubkeys has the offset to subkey List
                //tmpblock.subkey_count has number of subkeys. If its non zero, we read the offset block and add the subkey addres to stack along with its parent node
                #region
                t1nk += (int)tmpblock.subkey_count;
                if(tmpblock.subkey_count != 0)
                {
                    
                        listack.Enqueue(tmpblock.offsettosubkeys + root);
                        while (listack.Count != 0)
                        {
                            taddress = listack.Dequeue();
                            readoffset(taddress); // read offset data in off.
                            tcount = off.count;  //this should be similar to tmpblock.subkey_count
                            // taddress = tmpblock.offsettosubkeys;
                            hash = 8;  
                            lf = 0;
                            //if (off.block_type[0] != 'l' && (off.block_type[1] != 'f' || off.block_type[1] != 'h'))
                            if (off.block_type[0] != 'l')
                            {
                                //  if (off.block_type[1] != 'i' && (off.block_type[0] != 'r' || off.block_type[0] != 'l'))
                                if (off.block_type[1] != 'i')
                                {
                                    MessageBox.Show("Error reading file in lh, lf, ri, li section.");
                                    return;
                                }

                                // add the tcount address to liqueue
                                for (int j = 0; j < tcount; j++)
                                {
                                    lf = taddress + Marshal.SizeOf(off) + (j * 4);
                                    listack.Enqueue((int)readuintataddress(lf) + root);
                                }
                                lf = 1;

                            }
                            

                            if (lf == 0)  //li or ri type key
                            {
                                if (off.block_type[1] == 'i') hash = 4; // if li type value--- li is same as offset but without any hash
                                                                        // ri points to series of li, li points to nk values
                                for (int j = 0; j < tcount; j++)
                                {
                                    tmpuint = readuintataddress(taddress + Marshal.SizeOf(off) + (j * hash));

                                    //tmpuint has the offset for next NK block
                                    ad = (int)(tmpuint) + root;
                                    readkeyblock(ad);
                                    if (tmpblock.block_type[0] != 'n' && tmpblock.block_type[1] != 'k') { MessageBox.Show("NK value expected but not found."); return; }
                                    stackobject stk = new stackobject();
                                    stk.offset = ad;
                                    stk.treend = presentnode;
                                    jobstack.Enqueue(stk);

                                }  //for loop j=0;
                            }
                        }//while
                    

            } //if subkey count != 0
#endregion
            
            }
            l1.Text = "Node Keys: " + t1nk.ToString(); l1.Refresh();
            l2.Text = "Value Keys: " + t1vk.ToString(); l2.Refresh();
            l6.BackColor = this.BackColor;
            l6.Text = "Status: Done"; l6.Refresh();
            button2.Enabled = true;
           
          //  treeView1.ExpandAll();
            
            this.ResumeLayout();

        }
        private void openreghive2(object sender, EventArgs e)
        {
            t2nk = 0; t2vk = 0;
            OpenFileDialog fileOpen = new OpenFileDialog();

            fileOpen.InitialDirectory = ".\\";
            fileOpen.Filter = "All file (*.*)| *.*";
            fileOpen.FilterIndex = 1;
            fileOpen.ShowHelp = true;
          //  l.Text = "Total Number\n of key = 0";
            fileOpen.RestoreDirectory = false; //true;
            if (fileOpen.ShowDialog() == DialogResult.Cancel) return;
            string file = fileOpen.FileName; FileStream fs = null; ;
            try
            {
                fs = File.OpenRead(file);
            }
            catch
            {
                MessageBox.Show("Can not open file. In use."); return;
            }
            this.Refresh();
            this.SuspendLayout();
            data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            if (fs.Length < 4096) { MessageBox.Show("Not a reg hive file."); fs.Close(); return; }
            fs.Close();
            if (data[0] != 'r' || data[1] != 'e' || data[2] != 'g' || data[3] != 'f') { MessageBox.Show("Not a reg hive fike."); return; }
          //  if (treloaded != 0) { treeView1.Nodes.Clear(); treenode.Nodes.Clear(); treloaded = 1; }
            l6.BackColor = Color.Red;
            l6.Text = "Status: Reading Hive two.";
            l6.Refresh();
            this.button2.Enabled = false;
            // start by reading the registry header (only reads the file regname)
            string regn = readreghead();
            int presentkey = root + (int)fileregheader.offset;  //,4128
            int start = 0;
            string tstring = "", vstring = "";
            // add regname to the treeview
            treenode2 = new TreeNode();
             treeView2.Nodes.Add(treenode2);
           
            treenode2.Text = "--REGISTRY--ROOT--";
            treloaded = 1;

            //start the string search
            presentnode = new TreeNode();


            //add first job to stack
            tstackobject.offset = 4128;
            tstackobject.treend = treenode2;
            //  tstackobject.treend = null;
            jobstack.Enqueue(tstackobject);

            presentnode = treenode2;

            int tcount = 0, taddress = 0, ad = 0, iindex = 2, lf = 0, hash = 4;

            uint hex = 0x80000000;
            while (jobstack.Count != 0)
            {

                tstackobject = jobstack.Dequeue();// .Pop();
                presentkey = tstackobject.offset;
                presentnode = tstackobject.treend;
                readkeyblock(presentkey);  // read keyblock in tmpblock
                tstring = getstringataddress(presentkey + 80, tmpblock.len); //read the keyblock name. not necessary
                TreeNode newnode = new TreeNode();
                newnode.Text = tstring;
                if (start == 0) { start = 1; presentnode = treenode2; }  //avoid the first root hive name

                else
                {
                    //newnode.p
                    presentnode.Nodes.Add(newnode);
                    presentnode = newnode;
                }
                t2vk += tmpblock.value_count;

                //read all value keys in the tmpblock key block
                #region
                if (tmpblock.value_count != 0)
                {
                    int blankcount = 0;
                    for (int v = 0; v < tmpblock.value_count; v++)
                    {
                        // offset to value list is a series of address location (after 4 bytes of offset)
                        // these adress points to value blocks
                        ad = (int)readuintataddress(tmpblock.offsettovaluelist + root + 4 + 4 * v) + root;
                        readvalueblock(ad);
                        iindex = 2; //image index default to string type
                        if (tvalueblock.block_type[0] != 'v' || tvalueblock.block_type[1] != 'k') { MessageBox.Show("Value Block not VK type. Quitting."); return; }
                        if (tvalueblock.size > 0)
                        {
                            vstring = ""; TreeNode newnode1 = new TreeNode();
                            if (tvalueblock.name_len == 0) { blankcount++; vstring = "Default: "; } //the default key
                            else {   vstring = getstringataddress(ad + Marshal.SizeOf(tvalueblock), tvalueblock.name_len) + ": ";     }
                            newnode1.Tag = vstring.Length;
                                switch (tvalueblock.value_type)
                                {
                                    case 1: // simple string
                                    case 2: // "system root" type string keys
                                    case 6: //regkey link in string frmat                      
                                        if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; if (tvalueblock.size < 3) vstring += "BLANK"; else vstring = string.Concat(vstring, (char)tvalueblock.offset);  }
                                        else { vstring += getUTF16stringataddress((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); }
                                        break;

                                    case 7: //series of string terminated by 00 and also then fully terminated by 00
                                        if (tvalueblock.size >= hex) { vstring += "BLANK"; break; }
                                        vstring += getUTF16stringataddress((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break;
                                    case 4: // Dword 4 bytes . would normally be stored in offset value
                                        iindex = 1;
                                        if (tvalueblock.size >= hex) vstring += tvalueblock.offset.ToString();
                                        else vstring += "INVALID DWORD LENGTH";
                                        break;
                                    case 3: //binary data
                                    case 0:
                                    case 8:
                                    case 9:
                                    case 10:
                                    case 11:  // 64bit Endian Integer...Regedit marks this as binary value
                                        iindex = 1;
                                        if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; vstring += offsettostring(tvalueblock.offset, tvalueblock.size); }
                                        else { vstring += getstringataddressvalueof3((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break; }
                                        break;

                                    default: iindex = 1;
                                        if (tvalueblock.value_type < 15) MessageBox.Show("Use this file to add another value type");
                                        if (tvalueblock.size >= hex) { tvalueblock.size <<= 1; tvalueblock.size >>= 1; vstring += offsettostring(tvalueblock.offset, tvalueblock.size); }
                                        else { vstring += getstringataddressvalueof3((int)tvalueblock.offset + root + 4, (int)tvalueblock.size); break; }
                                        break;

                                } //switch
                            
                            if (blankcount > 1) { MessageBox.Show("More than one blank item in node/"); break; }
                         //   TreeNode newnode1 = new TreeNode();
                            newnode1.Text = vstring;

                            newnode1.ImageIndex = iindex;
                            newnode1.SelectedImageIndex = iindex;
                            presentnode.Nodes.Add(newnode1);
                        }
                    }
                }
                #endregion

                // read all subkeys in the tmpblock and add them to the stack
                //tmpblock.offsettosubkeys has the offset to subkey List
                //tmpblock.subkey_count has number of subkeys. If its non zero, we read the offset block and add the subkey addres to stack along with its parent node
                #region
                t2nk += (int)tmpblock.subkey_count;
                if (tmpblock.subkey_count != 0)
                {

                    listack.Enqueue(tmpblock.offsettosubkeys + root);
                    while (listack.Count != 0)
                    {
                        taddress = listack.Dequeue();
                        readoffset(taddress); // read offset data in off.
                        tcount = off.count;  //this should be similar to tmpblock.subkey_count
                        // taddress = tmpblock.offsettosubkeys;
                        hash = 8;
                        lf = 0;
                        //if (off.block_type[0] != 'l' && (off.block_type[1] != 'f' || off.block_type[1] != 'h'))
                        if (off.block_type[0] != 'l')
                        {
                            //  if (off.block_type[1] != 'i' && (off.block_type[0] != 'r' || off.block_type[0] != 'l'))
                            if (off.block_type[1] != 'i')
                            {
                                MessageBox.Show("Error reading file in lh, lf, ri, li section.");
                                return;
                            }

                            // add the tcount address to liqueue
                            for (int j = 0; j < tcount; j++)
                            {
                                lf = taddress + Marshal.SizeOf(off) + (j * 4);
                                listack.Enqueue((int)readuintataddress(lf) + root);
                            }
                            lf = 1;

                        }


                        if (lf == 0)  //li or ri type key
                        {
                            if (off.block_type[1] == 'i') hash = 4; // if li type value--- li is same as offset but without any hash
                            // ri points to series of li, li points to nk values
                            for (int j = 0; j < tcount; j++)
                            {
                                tmpuint = readuintataddress(taddress + Marshal.SizeOf(off) + (j * hash));

                                //tmpuint has the offset for next NK block
                                ad = (int)(tmpuint) + root;
                                readkeyblock(ad);
                                if (tmpblock.block_type[0] != 'n' && tmpblock.block_type[1] != 'k') { MessageBox.Show("NK value expected but not found."); return; }
                                stackobject stk = new stackobject();
                                stk.offset = ad;
                                stk.treend = presentnode;
                                jobstack.Enqueue(stk);

                            }  //for loop j=0;
                        }
                    }//while


                } //if subkey count != 0
                #endregion

            }
            //l.Text = "Total Numbers\n of Key = " + t1nk.ToString();
           // button2.Enabled = true;


            //  
            l4.Text = "Node Keys: " + t1nk.ToString(); l4.Refresh();
            l5.Text = "Value Keys: " + t1vk.ToString(); l5.Refresh();
           l6.Text ="Status: Comparing Files.\nPlease have patience.";
           l6.Refresh();
            button2.Enabled = false;
            button1.Enabled = false;
            this.Refresh();
            //Set the color of treenode1 to Red
            // SkyBlue = Added
            // Red = Deleted
            //Dark Green =  Same
            // Pink = changed
            TreeNode ttree;
            #region treenode set to Red and treenode1 set to SkyBlue

            l6.Text = "Status: Changing color to Red";
            l6.Refresh();
            treequeue.Enqueue(treenode);
            while (treequeue.Count != 0)
            {
                ttree = treequeue.Dequeue();
                 do
                {
                    ttree.BackColor = Color.Red;
                    if (ttree.FirstNode != null) treequeue.Enqueue(ttree.FirstNode);
                    ttree = ttree.NextNode;

                } while (ttree != null) ;
            }
            l6.Text = "Status: Changing color to Blue";
            l6.Refresh();
            treequeue.Enqueue(treenode2);
            while (treequeue.Count != 0)
            {
                ttree = treequeue.Dequeue();
                do
                {
                    ttree.BackColor = Color.SkyBlue;
                    if (ttree.FirstNode != null) treequeue.Enqueue(ttree.FirstNode);
                    ttree = ttree.NextNode;
                } while (ttree != null);
            }

            #endregion

            //start comparing node2 to node1
            l6.Text = "Status: Node: ";
            int ccnt = 0;
            l6.Refresh();
            treequeue.Enqueue(treenode2);
            while (treequeue.Count != 0)
            {ttree = treequeue.Dequeue();
                do
                {
                     stackthisnode = 1;
                    ccnt++; l6.Text = "Status:Checking Node: "+ccnt.ToString() + " of " +(t1nk +t1vk).ToString();
                            l6.Refresh();

                    findthisnodeintree1(ttree);
                    if (ttree.FirstNode != null && stackthisnode == 1) treequeue.Enqueue(ttree.FirstNode);
                    ttree = ttree.NextNode;
               
                } while (ttree != null);
            }
            treequeue.Clear();

            modifiedkeys = 0; addedkeys = 0; deletedkeys = 0; unchangedkeys = 0;
            treequeue.Enqueue(treenode);
            while (treequeue.Count != 0)
            {
                ttree = treequeue.Dequeue();
                do
                {
                    //ttree.BackColor = Color.Red;
                    if (ttree.BackColor == Color.Red) deletedkeys++;
                    if (ttree.BackColor == Color.SkyBlue) addedkeys++;
                    if (ttree.BackColor == Color.Pink) modifiedkeys++;
                    if (ttree.BackColor == Color.Green) unchangedkeys++;
                    if (ttree.FirstNode != null) treequeue.Enqueue(ttree.FirstNode);
                    ttree = ttree.NextNode;

                } while (ttree != null);
            }

            l8.Text = "Nodes Deleted       : " + deletedkeys.ToString();
            unchangedkeys -= 1; //to remove the root node created by this software
            l9.Text = "Nodes Unchanged : " + unchangedkeys.ToString();
            l10.Text = "Nodes Modified      : " + modifiedkeys.ToString();
            l11.Text = "Nodes Added         : " + addedkeys.ToString();
            l8.Refresh(); l9.Refresh(); l10.Refresh(); l11.Refresh();
            l6.BackColor = this.BackColor;
            l6.Text = "Status: Done.";
            l6.Refresh();

            nb = new Button();
            nb.Text = "Delete Common Data Nodes";
            nb.Size = new Size(190, 30);
            nb.Location = new Point(5, 455);
            nb.Click += new System.EventHandler(this.deletecommon);
            pnl.Controls.Add(nb);
            pnl.Refresh();
            this.ResumeLayout();
            
            treeView1.ResumeLayout();
            treeView1.Refresh();
      
        }
        TreeNode FinalNode = new TreeNode();
        void deletecommon(object sender, EventArgs e)
        {
            nb.Enabled = false;
            l6.BackColor = Color.Red;
            l6.Text = "Status: Deleting Data Nodes..";
            l6.Refresh();
            TreeNode tnn = new TreeNode(), tnn1=new TreeNode();;
            TreeNode ttree;
            Queue<TreeNode> newqueue = new Queue<TreeNode>();
            int start = 1;
   
            l6.Text = "Status: Deleting Key Nodes..";
            l6.Refresh();
            int anynodesdeleted = 2, cnot = 0;
            string ss = "";

          //  FinalNode = (TreeNode)treenode.Clone();
             treequeue.Clear();
            newqueue.Clear();start = 1;
     
            treequeue.Enqueue(treenode);
            FinalNode.Text = "--REGISTRY--ROOT--";//"--Registry--Root--";
            FinalNode.BackColor = Color.Green;
            FinalNode.Tag = null;
            newqueue.Enqueue(FinalNode);  
           
          //  anynodesdeleted = 0;

#region delete all value keys first. This is required so that we can delete empty nodes.
            while (treequeue.Count != 0)
            {
                ttree = treequeue.Dequeue();
                tnn = newqueue.Dequeue();
                do
                {
                    if (ttree.Tag != null && ttree.BackColor != Color.Green)  // if value key
                    {
                        tnn1 = new TreeNode();
                        tnn1.Text = ttree.Text;
                        tnn1.BackColor = ttree.BackColor;
                        tnn.Nodes.Add(tnn1);
                        tnn1.Tag = 1;
                        tnn1.ImageIndex = ttree.ImageIndex;
                    }
                    if (ttree.Tag == null )  //if node key
                    {
                        
                            if (ttree.FirstNode != null) treequeue.Enqueue(ttree.FirstNode);
                            tnn1 = new TreeNode(); tnn1.Text = ttree.Text;
                            tnn1.Tag = null;
                            tnn1.BackColor = ttree.BackColor;// Color.Green; 
                            if (start == 1) { start = 0; newqueue.Enqueue(FinalNode); }
                            else
                            { tnn.Nodes.Add(tnn1);
                                if (ttree.FirstNode != null) newqueue.Enqueue(tnn1);
                            }
                        
                   } 
                   try { ttree = ttree.NextNode; }
                    catch { ttree = null; }
                } while (ttree != null);
            }
       
        
#endregion

            //Now delete all the remaining node keys iteratively.
            while(anynodesdeleted !=0)
            {
            treequeue.Clear();
            newqueue.Clear();start = 1;
            treenode.Nodes.Clear();
            treenode = (TreeNode) FinalNode.Clone();
            FinalNode.Nodes.Clear();

            treequeue.Enqueue(treenode);
            FinalNode.Text = "--REGISTRY--ROOT--";//"--Registry--Root--";
            FinalNode.BackColor = Color.Green;
            FinalNode.Tag = null;
            newqueue.Enqueue(FinalNode);  
           
            anynodesdeleted = 0;
            while (treequeue.Count != 0)
            {
                ttree = treequeue.Dequeue();
                tnn = newqueue.Dequeue();
                do
                {
                    if (ttree.Tag != null && ttree.BackColor != Color.Green)  // if value key
                    {
                        tnn1 = new TreeNode();
                        tnn1.Text = ttree.Text;
                        tnn1.BackColor = ttree.BackColor;
                        tnn.Nodes.Add(tnn1);
                        tnn1.Tag = 1;
                        tnn1.ImageIndex = ttree.ImageIndex;
                    }
                    if (ttree.Tag == null )  //if node key
                    {
                        
                            if (ttree.FirstNode != null) treequeue.Enqueue(ttree.FirstNode);
                            tnn1 = new TreeNode(); tnn1.Text = ttree.Text;
                            tnn1.Tag = null;
                            tnn1.BackColor = ttree.BackColor;// Color.Green; 
                            if (start == 1) { start = 0; newqueue.Enqueue(FinalNode); }
                            else
                            {
                                if (ttree.BackColor == Color.Green && ttree.FirstNode == null) { ss = " " + ttree.Text; anynodesdeleted++; }
                                else tnn.Nodes.Add(tnn1);
                                if (ttree.FirstNode != null) newqueue.Enqueue(tnn1);
                            }
                        
                   } 
                    try { ttree = ttree.NextNode; }
                    catch { ttree = null; }
                } while (ttree != null);
            }
         }

           // 
            treeView1.Nodes.Clear();
            l6.BackColor = this.BackColor;
            l6.Text = "Status: Done.";
            l6.Refresh();
       
            treeView1.Nodes.Add(FinalNode);
            
          }
        void  findthisnodeintree1(TreeNode tn)
        {
            TreeNode ttree = new TreeNode(), temp=null, temp1=null; ;
            searchqueue.Clear();
            searchqueue.Enqueue(treenode);
            string fullpath = tn.FullPath;
            string shrt = "", nodetext="";
            int index = -1, comparestatus=0, last=0, level=0;
            string str2;

            if (tn.Tag == null)  //find only directory nodes. tag = null only for key nodes, contains value.length for value key
            {
                str2 = tn.Text;
                while (searchqueue.Count != 0)
                {
                    index = -1;
                    ttree = searchqueue.Dequeue();
                    if (ttree == null)
                    { stackthisnode = 0; 
                        temp1.Nodes.Add((TreeNode)tn.Clone()); 
                        return; }// this happens when the last node is empty, means items were added here
                    temp = ttree;
                    index = fullpath.IndexOf('\\'); last = 0;
                    if (index == -1) { shrt = fullpath; last = 1; }
                    else { shrt = fullpath.Substring(0, index); fullpath = fullpath.Substring(index + 1); }

                    do
                    {
                        found = 0;
                        if (ttree.Tag ==null)//we are not interested in value nodes
                        {
                            nodetext = ttree.Text;
                            comparestatus = string.Compare(shrt.ToUpper(), nodetext.ToUpper());
                            if (comparestatus == 0)
                            {
                                ttree.BackColor = Color.Green;
                                temp1 = ttree;  //save this tree in case its totally empty
                                searchqueue.Enqueue(ttree.FirstNode); //we will pump null in queue if the last node is empty
                                found = 1; 
                                if (last == 1) searchqueue.Clear(); 
                                break;
                           }
                        }
                        ttree = ttree.NextNode;
                    } while (ttree != null);

                    if (found == 0) //if the node is not found, means it was added in second file
                    {
                        stackthisnode = 0;
                        if (temp.Parent != null) { temp = temp.Parent; temp.Nodes.Add((TreeNode)tn.Clone()); }
                        else { treeView1.Nodes.Add((TreeNode)tn.Clone()); }
                    }
                }
            } //if tn is directory node
            else //str2 = tn.Text.Substring(0, (int)tn.Tag);  if incoming node is a value node.  this will happen only if that key node is present
            {
                level = -1;
                str2 = tn.Text.Substring(0, (int)tn.Tag); 
                while (searchqueue.Count != 0)
                {
                    index = -1;
                    ttree = searchqueue.Dequeue();
                    level++;
                    temp = ttree;
                    index = fullpath.IndexOf('\\'); last = 0; // value keys are allowed to have \\ in their names; check for this situation
                    if (index == -1) { shrt = tn.Text.Substring(0, (int)tn.Tag); ; last = 1; }
                    else { shrt = fullpath.Substring(0, index); fullpath = fullpath.Substring(index + 1); }
                    if (level == tn.Level) { level = 0; shrt = tn.Text.Substring(0, (int)tn.Tag); ; last = 1; }
                    do
                    {
                        found = 0;
                        if (ttree.Tag != null)//value nodes
                        {
                            nodetext = ttree.Text.Substring(0, (int)ttree.Tag);
                        }
                        else { nodetext = ttree.Text; } //key_block node

                            comparestatus = string.Compare(shrt.ToUpper(), nodetext.ToUpper());
                            if (comparestatus == 0) 
                            {
                                  ttree.BackColor = Color.Green;
                                  if (ttree.FirstNode != null) searchqueue.Enqueue(ttree.FirstNode); //this will only happen at the last node which we are not interested in anyways
                                  found = 1;
                            if (last == 1) { searchqueue.Clear();
                            if (string.Compare(ttree.Text.Substring((int)ttree.Tag).ToUpper(), tn.Text.Substring((int)tn.Tag).ToUpper()) != 0) { ttree.BackColor = Color.Pink; ttree.Text += (" _Changed to_ " + tn.Text.Substring((int)tn.Tag));}
                                            }
                                break; 
                            }
                        
                        ttree = ttree.NextNode;
                    } while (ttree != null);

                    if (found == 0) //if the node is not found
                    {
                        stackthisnode = 0;
                        if (temp.Parent != null) { temp = temp.Parent; temp.Nodes.Add((TreeNode)tn.Clone()); }
                        else { treeView1.Nodes.Add((TreeNode)tn.Clone()); }

                    }

                }
            }
        }
   
        uint  readuintataddress(int address)
        {
           uint tuint;
          tuint =  BitConverter.ToUInt32(data, address);
            return (tuint);
        }
        void readoffset(int address)  //reads a offset type structure at an addres
        {
            intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(off));
            Marshal.Copy(data, address, intPtr, Marshal.SizeOf(off));
            off = (offsets)Marshal.PtrToStructure(intPtr, typeof(offsets));
            Marshal.FreeHGlobal(intPtr);
        
        
        }
        void readkeyblock(int address)//reads a node type structure at an address
        {
           intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(tmpblock));
            Marshal.Copy(data, address, intPtr, Marshal.SizeOf(tmpblock));
            tmpblock = (key_block)Marshal.PtrToStructure(intPtr, typeof(key_block));
            Marshal.FreeHGlobal(intPtr);
           // return (tmpblock);
        }
        void readvalueblock(int address)  //reads a value type structure at an address
        {
           intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(tvalueblock));
           Marshal.Copy(data, address, intPtr, Marshal.SizeOf(tvalueblock));
            tvalueblock = (value_block)Marshal.PtrToStructure(intPtr, typeof(value_block));
            Marshal.FreeHGlobal(intPtr);
            // return (tmpblock);
        }
   

        string getstringataddress(int address, int len)  
        {
            byte[] tempbyte = new byte[len];
            Buffer.BlockCopy(data, address, tempbyte, 0, len);
            string str = System.Text.ASCIIEncoding.ASCII.GetString(tempbyte);
             return (str);
        }
 
        
       string getUTF16stringataddress(int address, int len)  
        {  // this has multiple strings. replace the end of string by space to easier presentation
            if (len % 2 != 0) { MessageBox.Show("Len not even. Taking default action"); len -= 1; }
            byte[] tempbyte = new byte[len];
            string str = ""; 
            Buffer.BlockCopy(data, address, tempbyte, 0, len);
            for (int i = 0; i < len; i+=2)
            {
               if (tempbyte[i] == 0 && tempbyte[i+1] == 0)
                {
                  tempbyte[i] = 32; 
                }
            } 
                  str = System.Text.UnicodeEncoding.Unicode.GetString(tempbyte, 0, len);
            return (str);
        }

              string getstringataddressvalueof3(int address, int len)  
              {
                  byte[] tempbyte = new byte[len];
                  Buffer.BlockCopy(data, address, tempbyte, 0, len);
                 string str = BitConverter.ToString(tempbyte).Replace("-", " ");
                  return (str);
              }
        
              string offsettostring(uint off, uint len)  
              {
                  byte[] tempbyte = BitConverter.GetBytes(off);
                  string str = BitConverter.ToString(tempbyte,0,(int)len).Replace("-", " ");
                  return (str);
              }

        private string readreghead() 
        {

            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fileregheader));
            Marshal.Copy(data, 0, intPtr, Marshal.SizeOf(fileregheader));
              fileregheader = (regheader)Marshal.PtrToStructure(intPtr, typeof(regheader));
              string s= getUTF16stringataddress(48, 64);
             Marshal.FreeHGlobal(intPtr);
            return (s);
       
        }

        public Form1()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }




    }
}
