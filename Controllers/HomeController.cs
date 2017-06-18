using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Novell.Directory.Ldap;


namespace IT_Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        
        private static string VerifyUser(string username, string password) {
            string searchBase = "ou=KSU,dc=KS,dc=mil,dc=no";
            int searchScope = LdapConnection.SCOPE_SUB; 
            string searchFilter = "(sAMAccountName="+ username +")";
            LdapConnection ldapConn= new LdapConnection();
            ldapConn.Connect("ks.mil.no",389);
            ldapConn.Bind("ks\\" + username,password);
            Console.WriteLine(ldapConn.Connected);
            string[] attr = {"sAMAccountName", "mail", "displayName", "department", "userPrincipalName", "title"};
            LdapSearchQueue queue=ldapConn.Search (searchBase,searchScope, searchFilter, attr,false,(LdapSearchQueue)null,(LdapSearchConstraints)null );
            LdapMessage message;
            StringBuilder sb = new StringBuilder();
            while ((message = queue.getResponse()) !=null)
            { 
                if (message is LdapSearchResult)
                {
                    LdapEntry entry = ((LdapSearchResult) message).Entry;
                    // System.Console.Out.WriteLine("\n" + entry.DN);
                    // System.Console.Out.WriteLine("\tAttributes: ");
                    LdapAttributeSet attributeSet =  entry.getAttributeSet();
                    System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
                    while(ienum.MoveNext())
                    {
                    LdapAttribute attribute=(LdapAttribute)ienum.Current;
                    string attributeName = attribute.Name;
                    string attributeVal = attribute.StringValue;
                    sb.AppendLine(attributeName + ": " + attributeVal);
                    }
                }
            }

            //Procced 

            //While all the required entries are parsed, disconnect   
            ldapConn.Disconnect();

            return sb.ToString();
        }
        private string getConfigInfo(string configItem){
            string confInfo = "";
            string configFile = System.IO.File.ReadAllText(_hostingEnvironment.ContentRootPath + @"\config.cfg");
            foreach (string line in configFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)) {
                if (line.Trim().StartsWith(configItem + ":")) {
                    confInfo = line.Substring(line.IndexOf(configItem + ":") + configItem.Length + 1).Replace("\"", "").Trim();
                }
            }
            return confInfo;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BrukerInfo(string username, string password)
        {            
            List<string> brukerInfos = new List<string>(VerifyUser(username, password).Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries));
            foreach (string brukerInfo in brukerInfos) {
                if (brukerInfo.StartsWith("sAMAccountName")) {
                    ViewData["sAMAccountName"] = brukerInfo.Replace("sAMAccountName: ", "");
                }else if (brukerInfo.StartsWith("mail")) {
                    ViewData["mail"] = brukerInfo.Replace("mail: ", "");
                }else if (brukerInfo.StartsWith("displayName")) {
                    ViewData["Title"] = brukerInfo.Replace("displayName: ", "");
                }else if (brukerInfo.StartsWith("department")) {
                    ViewData["department"] = brukerInfo.Replace("department: ", "");
                }else if (brukerInfo.StartsWith("userPrincipalName")) {
                    ViewData["userPrincipalName"] = brukerInfo.Replace("userPrincipalName: ", "");
                }else if (brukerInfo.StartsWith("title")) {
                    ViewData["userTitle"] = brukerInfo.Replace("title: ", "");
                } 
            }
            return View();
        }

        [HttpGet]
        public IActionResult HowTo(string id)
        {
            string configInfor = getConfigInfo("howto");
            DirectoryInfo d = new DirectoryInfo(@configInfor + "\\" + id);
            FileInfo[] Files = d.GetFiles("*.PDF");
            string[] directories = Directory.GetDirectories(@configInfor + "\\" + id);
            string[] subFolders = new string[directories.Length];
            string[] subFolderNames = new string[directories.Length];
            StringBuilder html = new StringBuilder();
            for (int i = 0; i < directories.Length; i++) { 
                subFolders[i] = directories[i].Replace(configInfor + "\\","");
                subFolderNames[i] = directories[i].Replace(configInfor + "\\","").Replace(id != null ? id + "\\" : " ", "");
            }
            string lastFolder;
            if (id == null) {
                lastFolder = "";
            }else if (id.LastIndexOf('\\') == -1) {
                lastFolder = "Root";
            }else {
                lastFolder = id.Remove(id.LastIndexOf('\\'));
            }
            foreach (FileInfo file in Files) { 
                string destDir = _hostingEnvironment.WebRootPath + @"\HowTo" + file.Directory.ToString().Replace(configInfor.Remove(configInfor.Length -1), "");
                if (!Directory.Exists(destDir)) {
                    Directory.CreateDirectory(destDir);
                }
                FileInfo destFile = new FileInfo(Path.Combine(destDir, file.Name));
                if (destFile.Exists)
                {
                    if (file.LastWriteTime > destFile.LastWriteTime)
                    { 
                        file.CopyTo(destFile.FullName, true);
                    }
                } else {
                    file.CopyTo(destFile.FullName);
                }
            }
            ViewBag.LastFolder = lastFolder;
            ViewBag.Folders = subFolders;
            ViewBag.FolderNames = subFolderNames;
            ViewBag.Length = subFolders.Length;
            for (int i = 0; i < Files.Length; i++) { 
                html.AppendLine("<a href=\"/HowTo" + Files[i].FullName.Replace(configInfor.Remove(configInfor.Length -1), "")
                .Replace("\\", "/") + "\" class=\"file\" target=\"_blank\"><button class=\"fileButton\"><img src=\"/images/pdf.png\"/>" + 
                Files[i].Name.Remove(Files[i].Name.Length -4) + "</button></a>");
            }
            ViewBag.Files = html.ToString();
            return View();
        }
        private string getFieldData(string path, string collection, string container, string ellement) {
            XDocument doc = XDocument.Parse(path.Replace("c:", ""));
            string value = "";
            try {
                value = doc.Root.Element(collection).Element(container).Element(ellement).Value;
            } catch (Exception e){
                Console.WriteLine(e);
            }
            return value;
        }

        public IActionResult Contact()
        {
            string configInfor = getConfigInfo("contact");
            DirectoryInfo d = new DirectoryInfo(@configInfor);
            FileInfo[] Files = d.GetFiles("*.contact");

            string[] emails = new string[Files.Length];
            string[] workPhones = new string[Files.Length];
            string[] jobTitles = new string[Files.Length];
            string[] picktures = new string[Files.Length];
            string[] fullNames = new string[Files.Length];
            string[] Messages = new string[Files.Length];
            for (int i = 0; i < Files.Length; i++) {
                FileInfo fileName = Files[i];
                string file = System.IO.File.ReadAllText(configInfor + fileName);

                string last ="";
                string middle ="";
                string first ="";
                last = getFieldData(file,"NameCollection","Name","FamilyName");
                middle = getFieldData(file,"NameCollection","Name","MiddleName");
                first = getFieldData(file,"NameCollection","Name","GivenName");
                emails[i] = getFieldData(file,"EmailAddressCollection","EmailAddress","Address");
                workPhones[i] = getFieldData(file,"PhoneNumberCollection","PhoneNumber","Number");
                jobTitles[i] = getFieldData(file,"PositionCollection","Position","JobTitle");
                picktures[i] = getFieldData(file,"PhotoCollection","Photo","Url");
                fullNames[i] = (last + ", " + first + " " + middle);
            }
            string[] pickturePaths  = new string[Files.Length];
            if (!System.IO.File.Exists(_hostingEnvironment.WebRootPath + @"\images\contact")) {
                System.IO.Directory.CreateDirectory(_hostingEnvironment.WebRootPath + @"\images\contact");
            }
            for (int i = 0; i < Files.Length; i++) {
                pickturePaths[i] = (_hostingEnvironment.WebRootPath + @"\images\contact\" + fullNames[i] + ".png");
                if (!System.IO.File.Exists(pickturePaths[i])) {
                    if (!System.IO.File.Exists(picktures[i])) {
                        System.Console.WriteLine("Path doesn't exist: {0}", picktures[i]);
                    } else {
                        System.IO.File.Copy(@picktures[i], @pickturePaths[i]);
                    }
                }
                pickturePaths[i] = (@"/images/contact/" + fullNames[i] + ".png");
            }
            ViewBag.length = Files.Length;
            ViewBag.email = emails;
            ViewBag.workPhone = workPhones;
            ViewBag.jobTitle = jobTitles;
            ViewBag.pickture = pickturePaths;
            ViewBag.fullName = fullNames;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
