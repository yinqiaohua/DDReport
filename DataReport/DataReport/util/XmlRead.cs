using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Fiddler;
namespace DataReport.util
{
    public class XmlRead
    {
        public XmlDocument doc;
        public string goalElem;
        public XmlRead(Fiddler .Session oSession)
        {
            doc = new XmlDocument();
            string response = Encoding.UTF8.GetString(oSession .ResponseBody);
            doc.LoadXml(response);
            //doc.LoadXml(oSession .ResponseBody.ToString());
        }
        

        public XmlNode getNode(string rootNode,string goalElem)
        {
            XmlNode xn = doc.SelectSingleNode(rootNode);
            XmlNodeList xnList = xn.ChildNodes;
            
            foreach (XmlNode xnl in xnList)
            {
                if (xnl.Name == goalElem)
                {
                    return xnl;
                }
            }
            return null;//找不到目标节点，就将根节点返回
 
        }
    }
    
    
}
