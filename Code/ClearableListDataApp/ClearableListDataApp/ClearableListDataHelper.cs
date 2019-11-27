using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ClearableListDataApp
{
    /// <summary>
    /// 添加、删除兼容性网站列表的代码
    /// </summary>
    public class ClearableListDataHelper
    {
        //兼容性列表在注册表中的位置，注意：不同位的操作系统可能不同，请注意测试。
        private const string CLEARABLE_LIST_DATA = @"Software\Microsoft\Internet Explorer\BrowserEmulation\ClearableListData";
        private const string USERFILTER = "UserFilter";
        private byte[] header = new byte[] { 0x41, 0x1F, 0x00, 0x00, 0x53, 0x08, 0xAD, 0xBA };
        private byte[] delim_a = new byte[] { 0x01, 0x00, 0x00, 0x00 };
        private byte[] delim_b = new byte[] { 0x0C, 0x00, 0x00, 0x00 };
        private byte[] checksum = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        private byte[] filler = BitConverter.GetBytes(DateTime.Now.ToBinary());//new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
        private byte[] regbinary = new byte[] { };

        /// <summary>
        /// 得到已经存在的所有兼容网站列表，如果没有，则返回空数组。
        /// </summary>
        /// <returns></returns>
        public string[] GetDomains()
        {
            string[] domains = { };
            using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(CLEARABLE_LIST_DATA))
            {
                //可能不存在此key.
                Object filterData = regkey.GetValue(USERFILTER);
                if (filterData != null)
                {
                    byte[] filter = filterData as byte[];
                    domains = GetDomains(filter);
                }
            }
            return domains;
        }

        /// <summary>
        /// 从byte数组中分析所有网站名称
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string[] GetDomains(byte[] filter)
        {
            List<string> domains = new List<string>();
            int length;
            int offset_filter = 24;
            int totalSize = filter.Length;
            while (offset_filter < totalSize)
            {
                length = BitConverter.ToUInt16(filter, offset_filter + 16);
                domains.Add(System.Text.Encoding.Unicode.GetString(filter, 16 + 2 + offset_filter, length * 2));
                offset_filter += 16 + 2 + length * 2;
            }
            return domains.ToArray();
        }

        /// <summary>
        /// 从兼容性列表中删除一个网站。
        /// </summary>
        /// <param name="domain">要删除网站</param>
        public void RemoveUserFilter(string domain)
        {
            String[] domains = GetDomains();
            if (!domains.Contains(domain))
            {
                return;
            }
            using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(CLEARABLE_LIST_DATA, true))
            {
                object oldData = regkey.GetValue(USERFILTER);
                if (oldData != null)
                {
                    byte[] filter = oldData as byte[];
                    byte[] newReg = GetRemovedValue(domain, filter);

                    if (GetDomains(newReg).Length == 0)
                        regkey.DeleteValue(USERFILTER);
                    else
                        regkey.SetValue(USERFILTER, newReg, RegistryValueKind.Binary);
                }
            }
        }

        /// <summary>
        /// 得到一个网站的存储的数据
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public byte[] GetRemovedValue(string domain, byte[] filter)
        {
            byte[] newReg;
            int length;
            int offset_filter = 24;
            int offset_newReg = 0;
            int totalSize = filter.Length;

            newReg = new byte[totalSize];
            Array.Copy(filter, 0, newReg, 0, offset_filter);
            offset_newReg += offset_filter;

            while (offset_filter < totalSize)
            {
                length = BitConverter.ToUInt16(filter, offset_filter + 16);
                if (domain != System.Text.Encoding.Unicode.GetString(filter, offset_filter + 16 + 2, length * 2))
                {
                    Array.Copy(filter, offset_filter, newReg, offset_newReg, 16 + 2 + length * 2);
                    offset_newReg += 16 + 2 + length * 2;
                }
                offset_filter += 16 + 2 + length * 2;
            }
            Array.Resize(ref newReg, offset_newReg);
            byte[] newSize = BitConverter.GetBytes((UInt16)(offset_newReg - 12));
            newReg[12] = newSize[0];
            newReg[13] = newSize[1];

            return newReg;
        }

        /// <summary>
        /// 向兼容性列表中添加一个网站
        /// </summary>
        /// <param name="domain"></param>
        public void AddNewSiteToCompatibilityViewList(String domain)
        {
            String[] domains = GetDomains();
            if (domains.Length > 0)
            {
                if (domains.Contains(domain))
                {
                    return;
                }
                else
                {
                    domains = domains.Concat(new String[] { domain }).ToArray();
                }
            }
            else
            {
                domains = domains.Concat(new String[] { domain }).ToArray();
            }

            int count = domains.Length;
            byte[] entries = new byte[0];
            foreach (String d in domains)
            {
                entries = this.Combine(entries, this.GetDomainEntry(d));
            }
            regbinary = header;
            regbinary = this.Combine(regbinary, BitConverter.GetBytes(count));
            regbinary = this.Combine(regbinary, checksum);
            regbinary = this.Combine(regbinary, delim_a);
            regbinary = this.Combine(regbinary, BitConverter.GetBytes(count));
            regbinary = this.Combine(regbinary, entries);
            Registry.CurrentUser.OpenSubKey(CLEARABLE_LIST_DATA, true).SetValue(USERFILTER, regbinary, RegistryValueKind.Binary);
        }

        /// <summary>
        /// 得到一个网站在兼容性列表中的数据，跟GetRemovedValue类似
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public byte[] GetDomainEntry(String domain)
        {
            byte[] tmpbinary = new byte[0];
            byte[] length = BitConverter.GetBytes((UInt16)domain.Length);
            byte[] data = System.Text.Encoding.Unicode.GetBytes(domain);
            tmpbinary = Combine(tmpbinary, delim_b);
            tmpbinary = Combine(tmpbinary, filler);
            tmpbinary = Combine(tmpbinary, delim_a);
            tmpbinary = Combine(tmpbinary, length);
            tmpbinary = Combine(tmpbinary, data);
            return tmpbinary;
        }

        /// <summary>
        /// 把两个byte[]数组合并在一起
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }
}