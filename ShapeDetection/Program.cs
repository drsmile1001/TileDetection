//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------
//
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;

namespace 磁磚辨識評分
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

            if (MessageBox.Show("是否進行辨識處理？", "執行項目", MessageBoxButtons.YesNo) == DialogResult.Yes)
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