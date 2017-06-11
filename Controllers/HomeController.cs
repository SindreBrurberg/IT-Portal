using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IT_Portal.Controllers
{
    public class HomeController : Controller
    {
        private string getConfigInfo(string configItem){
            string confInfo = "";
            string configFile = System.IO.File.ReadAllText("C:\\Users\\Sindre\\C#\\IT-Portal\\config.cfg");
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

        public IActionResult Contact()
        {
            Console.WriteLine(getConfigInfo("contact"));
            DirectoryInfo d = new DirectoryInfo(@getConfigInfo("contact"));
            FileInfo[] Files = d.GetFiles("*.contact");

            string[] email = new string[Files.Length];
            string[] workPhone = new string[Files.Length];
            string[] jobTitle = new string[Files.Length];
            string[] pickture = new string[Files.Length];
            string[] fullName = new string[Files.Length];
            string[] Message = new string[Files.Length];
            for (int i = 0; i < Files.Length; i++) {
                FileInfo fileName = Files[i];
                string file = System.IO.File.ReadAllText(getConfigInfo("contact") + fileName);
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
                    email[i] = doc.Root.Element("EmailAddressCollection").Element("EmailAddress").Element("Address").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    workPhone[i] = doc.Root.Element("PhoneNumberCollection").Element("PhoneNumber").Element("Number").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    jobTitle[i] = doc.Root.Element("PositionCollection").Element("Position").Element("JobTitle").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    pickture[i] = doc.Root.Element("PhotoCollection").Element("Photo").Element("Url").Value;
                } catch (Exception e){
                    Console.WriteLine(e);
                } try {
                    fullName[i] = (last + ", " + first + " " + middle);
                } catch (Exception e){
                    Console.WriteLine(e);
                }
            }

            ViewBag.length = Files.Length;
            ViewBag.email = email;
            ViewBag.workPhone = workPhone;
            ViewBag.jobTitle = jobTitle;
            ViewBag.pickture = pickture;
            ViewBag.fullName = fullName;

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
