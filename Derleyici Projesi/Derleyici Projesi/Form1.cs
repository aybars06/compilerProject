using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Derleyici_Projesi
{
    public partial class Form1 : Form
    {
        private string[] _tokens; // Dosyadan alýnan kelimeleri depolamak için dizi
        private List<Token> _tokenList = new List<Token>(); // Token nesnelerini depolamak için liste
        private List<Expression> _expressionList = new List<Expression>(); // Tokenlardan çözümlenen ifadeleri depolamak için liste

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Dosya içeriðini okuyup kelimelere böler
            string filePath = @"C:\Users\Aybars\Desktop\oku.txt"; // verinin okunduðu bilgisayarýmdaki dosya yolu 
            string fileContent = File.ReadAllText(filePath);
            _tokens = fileContent.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            textBox1.Text = fileContent; // Metni TextBox'a ata
        }

        int i = 0; // Belirli koþullarý takip etmek için sayaç

        private void Tokenize()
        {
            foreach (string token in _tokens)
            {
                string trimmedToken = token.Trim();

                // ; ile bitip 1 keyword tanýmlanmýþsa
                if (trimmedToken.EndsWith(";") && i == 1)
                {
                    // ; den önceki deðer ile ikisini bölme iþlemi yap.
                    string literalPart = trimmedToken.Substring(0, trimmedToken.Length - 1).Trim();
                    if (!string.IsNullOrEmpty(literalPart))
                    {
                        _tokenList.Add(new Token { Type = TokenType.Literal, Value = literalPart });
                    }
                    _tokenList.Add(new Token { Type = TokenType.Semicolon, Value = ";" });
                    if (IsNext(token))
                    {
                        i = 0; // ;den sonra baþka bir deðiþken tanýmlanýrsa sayacýn sýfýrlanmasý gerekiyor o yüzden yazmak gerekli.
                    }
                }
                else if (i > 1)
                {
                    listBox2.Items.Add(" Hata! Birden fazla anahtar kelime tanýmlandý..."); // Birden fazla anahtar kelime bir ; den önce tanýmlanýrsa hata mesajý göster
                    _tokenList.Clear(); // Token listesini temizle
                    break; //döngüden çýkmalý bir daha girmeyecek...
                }
                else
                {
                    // Boþluk karakterine göre tokenlarý ayýr
                    string[] parts = trimmedToken.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string part in parts)
                    {
                        if (IsKeyword(part))
                        {
                            if (i < 1)
                            {
                                _tokenList.Add(new Token { Type = TokenType.Keyword, Value = part });
                            }
                            else
                            {
                                _tokenList.Clear(); // Eðer 2.kez buraya girerse int int gibi 2 keyword tanýmý yapýlmýþtýr hata vermesi gerekiyor daha önceki aldýðý verileri silmesi için yazdým.
                            }
                            i++;
                        }
                        else if (i == 1) //sadece ; e kadar 1 keyword varsa...
                        {
                            if (IsIdentifier(part))
                            {
                                _tokenList.Add(new Token { Type = TokenType.Identifier, Value = part });
                            }
                            if (IsOperator(part))
                            {
                                _tokenList.Add(new Token { Type = TokenType.Operator, Value = part });
                            }
                            if (IsNumericLiteral(part))
                            {
                                _tokenList.Add(new Token { Type = TokenType.Literal, Value = part });
                            }
                        }
                    }
                }
            }
        }

        // Bir tokenýn sayýsal bir deðer olup olmadýðýný kontrol et
        private bool IsNumericLiteral(string token)
        {
            double result;
            return double.TryParse(token, out result);
        }

        // Bir tokenýn anahtar kelime olup olmadýðýný kontrol et
        private bool IsKeyword(string token)
        {
            const string keywordPattern = @"int|string|float|double";
            return Regex.IsMatch(token, keywordPattern);
        }

        // Bir tokenýn geçerli bir tanýmlayýcý olup olmadýðýný kontrol et
        private bool IsIdentifier(string token)
        {
            string pattern = @"[a-zA-Z_][a-zA-Z_0-9]*";
            return Regex.IsMatch(token, pattern);
        }

        // Bir tokenýn operatör olup olmadýðýný kontrol et
        private bool IsOperator(string token)
        {
            const string operatorPattern = @"[=+\-*/%]";
            return Regex.IsMatch(token, operatorPattern);
        }

        // ; den sonraki tokende keyword geliyorsa yazdýrmaya devam etmesi için ekledim. Eklemediðim takdirde birden fazla keyword kullandýnýz diyip veriyi okumuyordu.
        private bool IsNext(string token)
        {
            const string pattern = @";";
            return Regex.IsMatch(token, pattern);
        }

        // Tokenlardan ifadeleri çözümle
        private void EvaluateExpressions()
        {
            for (int i = 0; i < _tokenList.Count; i++)
            {
                Token token = _tokenList[i];
                if (token.Type == TokenType.Operator && i > 0 && i < _tokenList.Count - 1)
                {
                    Expression expression = new Expression();
                    expression.Operator = token.Value;
                    expression.LeftOperand = _tokenList[i - 1].Value;
                    expression.RightOperand = _tokenList[i + 1].Value;
                    _expressionList.Add(expression);
                }
            }
        }

        // Buton týklama olayý, tokenleþtirme ve deðerlendirme iþlemini baþlatýr
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear(); // Ýfadeler için listbox'ý temizle
            listBox2.Items.Clear(); // Tokenlar için listbox'ý temizle
            _expressionList.Clear(); // Önceki ifadeleri temizle
            Tokenize(); // Girdiyi tokenle
            EvaluateExpressions(); // Tokenlardan ifadeleri çözümle
            DisplayTokens(); // Tokenlarý listbox'ta göster
            DisplayExpressions(); // Ýfadeleri listbox'ta göster
        }

        // Tokenlarý listbox'ta göster
        private void DisplayTokens()
        {
            for (int i = 0; i < _tokenList.Count; i++)
            {
                Token token = _tokenList[i];
                string separator = "";

                if (i < _tokenList.Count - 1)
                {
                    Token nextToken = _tokenList[i + 1];
                    if (token.Type != nextToken.Type)
                    {
                        separator = " ";
                    }
                    else if (token.Type == TokenType.Semicolon)
                    {
                        separator = "\n";
                    }
                }

                string tokenType = token.Type.ToString();

                listBox2.Items.Add($"{token.Value} : {tokenType}{separator}");
            }
        }

        // Ýfadeleri listbox'ta göster
        private void DisplayExpressions()
        {
            listBox1.Items.Clear(); // Ýfadeleri eklemeden önce listbox'ý temizle
            foreach (Expression expression in _expressionList)
            {
                listBox1.Items.Add($"{expression.LeftOperand} {expression.Operator} {expression.RightOperand}");
            }
        }

        // Token sýnýfý, bir tokený temsil eder
        public class Token
        {
            public TokenType Type { get; set; }
            public string Value { get; set; }
        }

        // Ýfade sýnýfý, bir ifadeyi temsil eder
        public class Expression
        {
            public string LeftOperand { get; set; }
            public string RightOperand { get; set; }
            public string Operator { get; set; }
        }

        // Token türleri için enum
        public enum TokenType
        {
            Keyword,
            Identifier,
            Operator,
            Literal,
            Semicolon,
            next,
        }
    }
}