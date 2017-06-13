using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;


namespace IT_Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
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

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpGet]
        public IActionResult HowTo(string id)
        {
            string configInfor = getConfigInfo("howto");
            DirectoryInfo d = new DirectoryInfo(@configInfor + "\\" + id);
            FileInfo[] Files = d.GetFiles("*.PDF");
            //System.IO.Directory.GetDirectories(@configInfor,"*", System.IO.SearchOption.AllDirectories)
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
                        // now you can safely overwrite it
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
                html.AppendLine("<a href=\"/HowTo" + Files[i].FullName.Replace(configInfor.Remove(configInfor.Length -1), "").Replace("\\", "/") + "\" class=\"file\" target=\"_blank\"><button class=\"fileButton\"><img src=\"/images/pdf.png\"/>" + Files[i].Name.Remove(Files[i].Name.Length -4) + "</button></a>");
                //html.AppendLine("<li><a asp-area=\"\" asp-controller=\"Home\" asp-action=\"HowTo\">How to</a></li>"); 
                //html.AppendLine("<li><a herf=/" + directori.Replace(configInfor + "\\","") + "><button>" + directori.Replace(configInfor + "\\","") + "</button></a></li>");
            }
            ViewBag.Files = html.ToString();
            return View();
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
                XDocument doc = XDocument.Parse(file.Replace("c:", ""));

                string last ="";
                string middle ="";
                string first ="";
                try {
                    last = doc.Root.Element("NameCollection").Element("Name").Element("FamilyName").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    middle = doc.Root.Element("NameCollection").Element("Name").Element("MiddleName").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    first = doc.Root.Element("NameCollection").Element("Name").Element("GivenName").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    emails[i] = doc.Root.Element("EmailAddressCollection").Element("EmailAddress").Element("Address").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    workPhones[i] = doc.Root.Element("PhoneNumberCollection").Element("PhoneNumber").Element("Number").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    jobTitles[i] = doc.Root.Element("PositionCollection").Element("Position").Element("JobTitle").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    picktures[i] = doc.Root.Element("PhotoCollection").Element("Photo").Element("Url").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    fullNames[i] = (last + ", " + first + " " + middle);
                } catch (Exception e){
                    Console.WriteLine(e);
                }
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Files.Length; i++) {
                sb.AppendLine("<div class=\"gridInfo\">");
                    sb.AppendLine("<img src=\""+ pickturePaths[i] +"\" height=\"100\" width=\"100\" class=\"info\"/>");
                    sb.AppendLine("<div class=\"info\">");
                        sb.AppendLine("<h3 class=\"c\">"+ fullNames[i] +"</h3>");
                        sb.AppendLine("<h4 class=\"c\">"+ jobTitles[i] +"</h4>");
                        sb.AppendLine("<p class=\"c\">"+ emails[i] +"</p>");
                        sb.AppendLine("<p class=\"c\">"+ workPhones[i] +"</p>");
                    sb.AppendLine("</div>");
                sb.AppendLine("</div>");
            }
            ViewBag.Message = sb.ToString();
            // ViewBag.length = Files.Length;
            // ViewBag.email = emails;
            // ViewBag.workPhone = workPhones;
            // ViewBag.jobTitle = jobTitles;
            // ViewBag.pickture = pickturePaths;
            // ViewBag.fullName = fullNames;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
