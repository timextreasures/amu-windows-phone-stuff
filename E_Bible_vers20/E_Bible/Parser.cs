using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;  // AMU for parsering HTML needs also reference System.Xml.Ling.dll (XDocument)
using HtmlAgilityPack; // Amu 22.03.2012: instructions: http://www.jnjdev.com/forums/topic6-html-agility-pack-for-windows-phone-7-wp7.aspx


namespace E_Bible
{
    // used for gathering Chapter - name / html - hyperlink pairs for main glossary - view
    public class glossaryInfo
    {

        public String header { get; set;}
        public String content { get; set; }
        
    }

    public class Parser
    {
        private String outputStr = "";
        public String output
        {
            get { return outputStr; }
            set { outputStr = value; }
        }

        private int amountOfPages = 0;
        public int pageCount { get { return amountOfPages; } }

        public void insert(String str)
        {
            output = output + str + Environment.NewLine; // set { outputStr = output + str + Environment.NewLine; }
        }
        
        
        /// <summary>
        /// Parse the start point for Bible book - htmls are splitted and the starting point is some rare cases somewhere inside the content
        /// </summary>
        /// <param name="content">Original HTML content</param>
        /// <param name="BibleBook">Shortened content, start point in the start of chapter</param>
        /// <returns></returns>
        public String startPoint(String content, String BibleBook)
        {
            content = content.Substring(content.IndexOf(BibleBook), content.Length);
            return content;
        }

        /// <summary>
        /// Split the handled Bible book string to one size of one textBlock (page)
        /// </summary>
        /// TODO check the nodes which perhaps has been left from previous splitting (eg. </xx>)
        /// For some reason sometimes text is cutted just few lines before end of String text on last page, ccheck the reason (in case chapter7 when there is even number of pages 17)
        /// <param name="text"></param>
        /// <returns>Splitted texts in String array</returns>
        public List<String> splitTheTextToPages(String text)
        {
            String headPart = "", bookContent = "";
            int lineCounter = 0, textPageIndex = 0, pageTxtLenght = 0, startOfBook = 0;
            string oneLineText = "", onePageContent = "", extraBigLeftOver = "";
            HtmlDocument doc = new HtmlDocument();
            bool splitOnlyOnce = false;

            // Splitting lines is temporaly out of use, works ok but functionality is not the best possible 
            //text = splitExtraLongLines(text);
            doc.LoadHtml(text);

            // Lets save the main staff (css, version + other info) to saved to start of each splitted html
            // This achieve out of memory in case of Lilliput ePub
            headPart = doc.DocumentNode.InnerHtml.Substring(0, doc.DocumentNode.InnerHtml.IndexOf("<body>"));
            // Save the actual html content of Bible book
            bookContent = doc.DocumentNode.InnerHtml.Substring(doc.DocumentNode.InnerHtml.IndexOf("<body>")); // for some reason this cuts many cases 

            // Used with pg10.epub KJV Bible - TODO needs to be handled in different way in future implementations (if Chapter does not start in the beginning of HTML content)
            if (bookContent.Contains("pgepubid000")) // in KJV Bible this wqas the start of Chapter - node
                startOfBook = bookContent.IndexOf("<h", (bookContent.IndexOf("pgepubid000") - 10)); // TODO "-10"
            else
                startOfBook = 0;
            // Set the Bible book's starting point 
            bookContent = bookContent.Substring(startOfBook); 

            List<String> splittedHtml = new List<string>(); // TODO can this be static - not new ??
            
            using (StringReader sReader = new StringReader(bookContent)) //(text))
            {

                while (true) // until ReadLine = null
                {
 
                    oneLineText = sReader.ReadLine(); 

                    if (oneLineText == null)
                        break;

                    else
                    {
                        pageTxtLenght = pageTxtLenght + oneLineText.Length;
                    }

                    // Combine also end of previous splitted extrabig content
                    onePageContent = onePageContent + oneLineText + Environment.NewLine;
                    //
                    extraBigLeftOver = "";
                    splitOnlyOnce = false;

                    lineCounter++;

                    if (lineCounter == 7 || pageTxtLenght > 450)
                    {

                        // font size
                        onePageContent = onePageContent.Insert(0, "<font size=\"4\">");
                        onePageContent = onePageContent.Insert(onePageContent.Length, "</font>");

                        // Add the header part 
                        // TODO: do we even need this headPart in HTML content?
                     //   onePageContent = headPart + onePageContent;

                        splittedHtml.Insert(textPageIndex, FixBrokenTags(onePageContent));
                        textPageIndex++;
                        lineCounter = 0;
                        onePageContent = "";
                        pageTxtLenght = 0;
                        oneLineText = "";
                    }   
                }
            }
            //foreach (String oneText in splittedTexts)
            amountOfPages = textPageIndex;
            StaticDataForPageChange.amountOfPages = textPageIndex + 1; // textPageIndex starts from zero
            return splittedHtml;

        }

        /// <summary>
        /// NOT USED YET
        /// PageTurn class needs information when user is turning page is there more pages available
        /// </summary>
        /// <returns></returns>
        public bool isThereContentBefore()
        {
            return true;
        }

        /// <summary>
        /// Purpose is cut extra long lines in HTML. Too long lines achieve problems during page splitting
        ///  TODO: logic for splitting needs to be changed. Now split happens when next ". " has founded after 200 marks.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public String splitExtraLongLines(String content)
        {
            int startPoint = 0, endPoint = 0;
            String tempContent = "";
            String [] splittedContent = new String[50];
            int i = 0;

            // Some e.g. first title links dont contain <p> paragraphs
            if(content.Contains("<p ") || content.Contains("<p>"))
            {
                while (true)
                {
  
                    // Start of string in first chapter
                    if (content.IndexOf("<p>") == -1)
                        tempContent = content.Substring(content.IndexOf("<p "));
                    else
                        tempContent = content.Substring(content.IndexOf("<p>"));

                    tempContent = tempContent.Substring(0, tempContent.IndexOf("</p>") + 4);

                    startPoint = content.IndexOf("<p>", startPoint + 4);
                    endPoint = content.IndexOf("</p>", endPoint + 4);

                    if (endPoint == -1)
                        break;
                    // Nearest dot "."
                    if ((endPoint - startPoint) > 500 && tempContent.Contains(". "))
                    {
                       // splittedContent[i] = tempContent.Substring(tempContent.IndexOf(". ", 200) + 2);
                        content = content.Insert(content.IndexOf(". ", (startPoint + 200)), "</p>" + Environment.NewLine + "<p>"); // + 1 = " . "
                    }

                    i++;
                }
            }
            return content;
        }
        /// <summary>
        /// TODO check and clean left - over tags from previous HTML
        /// Missing tags are added to "broken" splitted html content
        /// </summary>
        /// <param name="splittedHTML"></param>
        /// <returns>splittedHTML String List</returns>
        public String FixBrokenTags(String splittedHTML)
        {
            int positionForNextTag = 0;

            HtmlDocument underConstruction = new HtmlDocument();
            underConstruction.LoadHtml(splittedHTML);

            bool anyProblems = true; // underConstruction.OptionCheckSyntax;
            underConstruction.OptionFixNestedTags = true;
            underConstruction.OptionCheckSyntax = true;
            underConstruction.OptionAutoCloseOnEnd = true;

            if (anyProblems)
            {
             //   errors = underConstruction.ParseErrors.Count();
                foreach (HtmlParseError err in underConstruction.ParseErrors)
                {

                    String errCode = err.Code.ToString();
                    String errReason = err.Reason.ToString();
                    /*        String errText = err.SourceText.ToString();
                            string errLine =  err.Line.ToString();
                            String errStreamPos = err.StreamPosition.ToString();
                            String errLinePos = err.LinePosition.ToString();

                    
                            uint line = Convert.ToUInt32(errLine);
                            uint streamPos = Convert.ToUInt32(errStreamPos);
                            */
                    //   underConstruction.CreateTextNode("</html>");
                    // Tags needs to be added before previous one
                    int tagValueStart = errReason.IndexOf("<");
                    int tagValueLenght = errReason.IndexOf(">") - (tagValueStart - 1);
                    splittedHTML = splittedHTML.Insert((splittedHTML.Length - positionForNextTag), errReason.Substring(tagValueStart, tagValueLenght)); // +Environment.NewLine;

                    positionForNextTag = positionForNextTag + tagValueLenght;
                }

            }
            return splittedHTML;
        }

        /// <summary>
        /// New functionality to create main glossary view. Parse from TOC - file all chapter - headers and html file links and make them as hyperlinks
        /// Old version use KJV Bible's first html - file which actually list all hyperlinks to every chapter 
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        public String mainGlossaryViewFromTOCfile(String tocContent)
        {

            // List all found Chapter header - Hyperlink pairs, "content" node contains the hyprlink name
         //   List<glossaryInfo> glossaryList = listOfHeaderAndHyperlink("content", tocContent);

            List<glossaryInfo> glossaryList = new List<glossaryInfo>();

            String bookHeader = "", hyperlink = "", chapterName = "Book's header not found";
            // When we are reading html, xml - parser dont like this tag: DOCTYPE !! ugly way to fix is use .Remove(...) - TODO change
            // Possible resolution, use parametr settings: XmlReader.Create(new StringReader(tocContent), settings)
            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.DtdProcessing = DtdProcessing.Parse;
            //settings.ValidationType = ValidationType.DTD;
            //settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            if (tocContent.Contains("<!DOCTYPE"))
            {
                // TODO: FIX ".IndexOf(".dtd")" SOMETIMES WITH SOME EPUB NEEDED .IndexOf(".dtd")+1)
                tocContent = tocContent.Remove(tocContent.IndexOf("<!DOCTYPE"), ((tocContent.IndexOf(".dtd") ) - tocContent.IndexOf("<!DOCTYPE") + 7)); // +1 = "> or '> 
            }
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StringReader(tocContent))) //  xmlString)))
            {
                bool match = false, match2 = false, startParse = false;
                // Parse the file and display each of the nodes.
                while (reader.Read()) // && match == false)
                {
                    // Lets skip the header and author info / navMap contains the book headers and hyperlinks HTMLs
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "navMap")
                    {
                        startParse = true;
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "docTitle")
                    {
                        bookHeader = reader.ReadInnerXml();
                        bookHeader = bookHeader.Substring(bookHeader.IndexOf(">") + 1);
                        bookHeader = bookHeader.Remove(bookHeader.IndexOf("<"));
                    }
                    if (startParse)
                    {
                        switch (reader.NodeType)
                        {

                            // Header for the chapter of mainview glossary
                            case XmlNodeType.Text:
                                //  if (reader.NodeType == XmlNodeType.Text && reader.Name == "text")
                                chapterName = reader.Value;
                                match = true;
                                break;

                            // HTML hyperlink 
                            case XmlNodeType.Element:
                                // we are inteterested <content> node's attribute in oct file (hyperlink name)
                                if (reader.Name == "content")
                                {

                                    // opf file parsering help in: http://idpf.org/epub/30/spec/epub30-publications.html
                                    hyperlink = reader.GetAttribute(0);
                                    match2 = true;
                                }
                                break;

                        }
                        if (match && match2)
                        {
                            glossaryList.Add(new glossaryInfo { content = hyperlink, header = chapterName });
                       //     StaticDataForPageChange.BibleChapterName = chapterName;
                            match = false;
                            match2 = false;
                        }
                    }

                }
            }

            String generatedList = "<center>" + "<h2>" + bookHeader + "</h2>" + "<br/>" + Environment.NewLine; // <center> glossary will be centerized

            for (int i = 0; i < glossaryList.Count; i++)
            {
                generatedList = generatedList + "<a href=\"" + glossaryList[i].content + "\">" + glossaryList[i].header + "</a><br/>" + Environment.NewLine;
                
            }

            generatedList = generatedList + "</center>";

            StaticDataForPageChange.ChapterNameList = glossaryList;

            return generatedList;

        }
        
    }

}
