using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;

using ImageTypers;
using System.IO;

namespace recaptcha_automation
{
    class Program
    {
        #region credentials
        private static string IMAGETYPERS_TOKEN = "2A1D2EC9DEA54B048ECF65A1B5EE8ADA";
        #endregion
        //35190803238055000122550010000054401781732361
        #region Test URLs
        private static string TEST_PAGE_NORMAL = "https://imagetyperz.xyz/automation/recaptcha-v2.html";
        private static string TEST_PAGE_REQUESTS_VERIFY = "https://imagetyperz.xyz/automation/recaptcha-verify.php";       // normally gathered dynamically
        private static string TEST_PAGE_INVISIBLE = "https://imagetyperz.xyz/automation/recaptcha-invisible.html";
        private static string TEST_PAGE_receita = "https://www.cte.fazenda.gov.br/portal/consultaRecaptcha.aspx?tipoConsulta=completa&tipoConteudo=mCK/KoCqru0=";

        #endregion

        #region normal
        /// <summary>
        /// Browser test recaptcha normal
        /// </summary>
        private static void browser_test_normal()
        {
            Console.WriteLine("[=] BROWSER TEST STARTED (NORMAL CAPTCHA) [=]");
            var s = ChromeDriverService.CreateDefaultService();
            s.HideCommandPromptWindow = true;
            ChromeDriver d = new ChromeDriver(s);
            try
            {
                d.Navigate().GoToUrl(TEST_PAGE_NORMAL);               // go to normal test page
                // complete regular data
                d.FindElementByName("username").SendKeys("rafaelportal@sowconnect.com");
                d.FindElementByName("password").SendKeys("113001");
                Console.WriteLine("[+] Completed regular info");
                // ---------------------

                // get sitekey
                string site_key = d.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));

                // complete captcha

                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_NORMAL);
                p.Add("sitekey", site_key);
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);

                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // set g-response-code in page source (with javascript)
                IJavaScriptExecutor e = (IJavaScriptExecutor)d;
                string javascript_code = string.Format("document.getElementById('g-recaptcha-response').innerHTML = '{0}';", g_response_code);
                e.ExecuteScript(javascript_code);
                Console.WriteLine("[+] Code set in page");

                // submit form
                d.FindElementByTagName("form").Submit();
                Console.WriteLine("[+] Form submitted");
            }
            finally
            {
                Thread.Sleep(5000);
                d.Quit();       // quit browser
                Console.WriteLine("[=] BROWSER TEST FINISHED [=]");
            }
        }
        /// <summary>
        /// Requests test recaptcha normal
        /// </summary>
		private static void requests_test_normal()
        {
            Console.WriteLine("[=] REQUESTS TEST STARTED (NORMAL RECAPTCHA) [=]");
            try
            {
                Console.WriteLine("[+] Getting sitekey from test page...");
                string resp = get(TEST_PAGE_NORMAL);    // download page first (to get sitekey)
                HtmlDocument d = new HtmlDocument();
                d.LoadHtml(resp);

                // get sitekey
                string site_key = d.DocumentNode.SelectSingleNode("//div[@class='g-recaptcha']").GetAttributeValue("data-sitekey", "");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));

                // complete captcha
                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_NORMAL);
                p.Add("sitekey", site_key);
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);
                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // create post request data
                string data = string.Format(
                    "username=rafaelportal@sowconnect.com&" +
                    "password=113001&" +
                    "g-recaptcha-response={0}",
                    g_response_code);

                // submit
                string response = post(TEST_PAGE_REQUESTS_VERIFY, data);
                Console.WriteLine(string.Format("[+] Response: {0}", response));
            }
            finally
            {
                Console.WriteLine("[=] REQUESTS TEST FINISHED [=]");
            }
        }
        #endregion

        #region invisible
        /// <summary>
        /// Browser test recaptcha invisible
        /// </summary>
        private static void browser_test_invisible()
        {
            Console.WriteLine("[=] BROWSER TEST STARTED (INVISIBLE CAPTCHA) [=]");
            var s = ChromeDriverService.CreateDefaultService();
            s.HideCommandPromptWindow = true;
            ChromeDriver d = new ChromeDriver(s);
            try
            {
                d.Navigate().GoToUrl(TEST_PAGE_INVISIBLE);               // go to normal test page
                                                                         // complete regular data
                d.FindElementByName("username").SendKeys("my-username");
                d.FindElementByName("password").SendKeys("password-here");
                Console.WriteLine("[+] Completed regular info");
                // ---------------------

                // get sitekey
                string site_key = d.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                string callback_method = d.FindElementByClassName("g-recaptcha").GetAttribute("data-callback");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));
                Console.WriteLine(string.Format("[+] Callback method: {0}", callback_method));

                // complete captcha
                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_NORMAL);
                p.Add("sitekey", site_key);
                p.Add("type", "2");
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);
                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // set g-response-code in page source (with javascript)
                IJavaScriptExecutor e = (IJavaScriptExecutor)d;
                string javascript_code = string.Format("document.getElementById('g-recaptcha-response').innerHTML = '{0}';", g_response_code);
                e.ExecuteScript(javascript_code);
                Console.WriteLine("[+] Code set in page");
                // submit form
                e.ExecuteScript(string.Format("{0}(\"{1}\");", callback_method, g_response_code));
                Console.WriteLine("[+] Callback function executed");
            }
            finally
            {
                Thread.Sleep(5000);
                d.Quit();       // quit browser
                Console.WriteLine("[=] BROWSER TEST FINISHED [=]");
            }
        }

        /// <summary>
        /// Requests test recaptcha invisible
        /// </summary>
		private static void requests_test_invisible()
        {
            ////button[@class="g-recaptcha"]
            Console.WriteLine("[=] REQUESTS TEST STARTED (INVISIBLE RECAPTCHA) [=]");
            try
            {
                Console.WriteLine("[+] Getting sitekey from test page...");
                string resp = get(TEST_PAGE_INVISIBLE);    // download page first (to get sitekey)
                HtmlDocument d = new HtmlDocument();
                d.LoadHtml(resp);

                // get sitekey
                string site_key = d.DocumentNode.SelectSingleNode("//button").GetAttributeValue("data-sitekey", "");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));

                // complete captcha
                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_NORMAL);
                p.Add("sitekey", site_key);
                p.Add("type", "2");
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);
                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // create post request data
                string data = string.Format(
                    "username=my-username&" +
                    "password=password-here&" +
                    "g-recaptcha-response={0}",
                    g_response_code);

                // submit
                string response = post(TEST_PAGE_REQUESTS_VERIFY, data);
                Console.WriteLine(string.Format("[+] Response: {0}", response));
            }
            finally
            {
                Console.WriteLine("[=] REQUESTS TEST FINISHED [=]");
            }
        }

        private static void abreReceita()
        {
            Console.WriteLine("[=] BROWSER TEST STARTED (NORMAL CAPTCHA) [=]");
            var s = ChromeDriverService.CreateDefaultService();
            s.HideCommandPromptWindow = true;
            ChromeDriver d = new ChromeDriver(s);
            try
            {
                d.Navigate().GoToUrl(TEST_PAGE_receita);               // go to normal test page
                                                                       // complete regular data
                var xID = "ctl00_ContentPlaceHolder1_txtChaveAcessoCompleta";

                d.FindElementById(xID).SendKeys("35190700214121000217570010005716071769317990");
                //d.SwitchTo().Frame(0);
                //d.FindElementById("recaptcha-anchor").Click();

                //d.FindElementByName("password").SendKeys("113001");
                //Console.WriteLine("[+] Completed regular info");
                // ---------------------

                // get sitekey
                string site_key = d.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));

                // complete captcha

                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_receita);
                p.Add("sitekey", site_key);
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);

                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // set g-response-code in page source (with javascript)
                IJavaScriptExecutor e = (IJavaScriptExecutor)d;
                string javascript_code = string.Format("document.getElementById('g-recaptcha-response').innerHTML = '{0}';", g_response_code);
                e.ExecuteScript(javascript_code);
                Console.WriteLine("[+] Code set in page");

                d.FindElementById("ctl00_ContentPlaceHolder1_btnConsultar").Click();

                // submit form
                d.FindElementByTagName("form").Submit();
                Console.WriteLine("[+] Form submitted");
            }
            finally
            {
                Thread.Sleep(5000);
                // d.Quit();       // quit browser
                Console.WriteLine("[=] BROWSER TEST FINISHED [=]");
            }
        }

        private static void abreReceitaIE()
        {
            Console.WriteLine("[=] BROWSER TEST STARTED (NORMAL CAPTCHA) [=]");

            //var s = ChromeDriverService.CreateDefaultService();
            //s.HideCommandPromptWindow = true;
            var options = new InternetExplorerOptions();

            var driverService = InternetExplorerDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            var d = new InternetExplorerDriver(driverService, new InternetExplorerOptions());

            // ChromeDriver d = new ChromeDriver(s);
            try
            {
                d.Navigate().GoToUrl(TEST_PAGE_receita);               // go to normal test page
                                                                       // complete regular data
                var xID = "ctl00_ContentPlaceHolder1_txtChaveAcessoCompleta";

                d.FindElementById(xID).SendKeys("35190700214121000217570010005716071769317990");
                //d.SwitchTo().Frame(0);
                //d.FindElementById("recaptcha-anchor").Click();

                //d.FindElementByName("password").SendKeys("113001");
                //Console.WriteLine("[+] Completed regular info");
                // ---------------------

                // get sitekey
                string site_key = d.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                Console.WriteLine(string.Format("[+] Site key: {0}", site_key));

                
                // complete captcha

                Console.WriteLine("[+] Waiting for recaptcha to be solved ...");
                ImageTypersAPI i = new ImageTypersAPI(IMAGETYPERS_TOKEN);
                Dictionary<string, string> p = new Dictionary<string, string>();
                p.Add("page_url", TEST_PAGE_receita);
                p.Add("sitekey", site_key);
                string recaptcha_id = i.submit_recaptcha(p);       // submit recaptcha info
                // while in progress, sleep for 10 seconds
                while (i.in_progress(recaptcha_id)) { Thread.Sleep(10000); }
                string g_response_code = i.retrieve_captcha(recaptcha_id);

                //Console.Write("CODE:"); Console.ReadLine(); string g_response_code = File.ReadAllText("g-response.txt");        // get manually
                Console.WriteLine(string.Format("[+] Got g-response-code: {0}", g_response_code));

                // set g-response-code in page source (with javascript)
                IJavaScriptExecutor e = (IJavaScriptExecutor)d;
                string javascript_code = string.Format("document.getElementById('g-recaptcha-response').innerHTML = '{0}';", g_response_code);
                e.ExecuteScript(javascript_code);
                Console.WriteLine("[+] Code set in page");

                
                ////input[@id='ctl00_ContentPlaceHolder1_btnConsultar']
                /// name = ctl00$ContentPlaceHolder1$btnConsultar
                /// id = ctl00_ContentPlaceHolder1_btnConsultar
                /// 
                try
                {
                    System.Threading.Thread.Sleep(7000);

                    var button = d.FindElementByCssSelector("input[value='Continuar']");

                  
                    button.Click();
                    button.Click();

                    d.ExecuteScript("javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions('ctl00$ContentPlaceHolder1$btnConsultar', '', true, 'completa', '', false, false))");

                    Console.WriteLine("[+] btnConsulta submitted");
                }
                catch { }


                // javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions("ctl00$ContentPlaceHolder1$btnConsultar", "", true, "completa", "", false, false))
                // submit form
                // d.FindElementByTagName("form").Submit();
                //  Console.WriteLine("[+] Form submitted");

                // botao download  ctl00_ContentPlaceHolder1_btnDownload
                Console.WriteLine("[+] botao download");
                System.Threading.Thread.Sleep(8000);
                //Download do documento
                var buttonDownload = d.FindElementByCssSelector("input[value='Download do documento']");
                buttonDownload.Click();
                //d.FindElementById("ctl00_ContentPlaceHolder1_btnDownload").Click();
            }
            catch(Exception ex)
            {
                string erro = ex.Message;
            }
            finally
            {
                Thread.Sleep(5000);
                // d.Quit();       // quit browser
                Console.WriteLine("[=] BROWSER TEST FINISHED [=]");
            }
        }
        #endregion

        #region utils
        /// <summary>
        /// GET request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string get(string url)
        {
            using (WebClient wb = new WebClient())
            {
                return wb.DownloadString(url);
            }
        }
        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string post(string url, string post_data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(post_data);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            request.Accept = "*/*";
            //request.ServicePoint.Expect100Continue = false;
            //request.AllowAutoRedirect = false;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            string s = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return s;
        }
        #endregion

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("[==] TESTS STARTED [==]");

                // abreReceita();
                abreReceitaIE();
                //browser_test_normal();
                //Console.WriteLine("--------------------------------------------------------------------");
                //requests_test_normal();
                //Console.WriteLine("--------------------------------------------------------------------");
                //browser_test_invisible();
                //Console.WriteLine("--------------------------------------------------------------------");
                //requests_test_invisible();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[!] Error occured: {0}", ex.Message));
                Console.WriteLine("[==] ERROR [==]");
            }
            finally
            {
                Console.WriteLine("[==] TESTS FINISHED [==]");
                Console.ReadLine();
            }
        }
    }
}
