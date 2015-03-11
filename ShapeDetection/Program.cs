//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;

namespace �Ͽj���ѵ���
{
    public static class Program
    {
        public static IdentificationForm myIDForm;
        public static FormUserDef myUserDefForm;

        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (MessageBox.Show("�O�_�i����ѳB�z�H", "���涵��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                myUserDefForm = new FormUserDef();
                Application.Run(myUserDefForm);
            }
            else
            {
                Application.Run(new SampleGen());
            }  


            
        }
    }
}