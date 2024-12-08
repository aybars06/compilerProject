using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Derleyici_Projesi
{
    public partial class Form1 : Form
    {
        private string[] _tokens; // Dosyadan al�nan kelimeleri depolamak i�in dizi
        private List<Token> _tokenList = new List<Token>(); // Token nesnelerini depolamak i�in liste
        private List<Expression> _expressionList = new List<Expression>(); // Tokenlardan ��z�mlenen ifadeleri depolamak i�in liste

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Dosya i�eri�ini okuyup kelimelere b�ler
            string filePath = @"C:\Users\Aybars\Desktop\oku.txt"; // verinin okundu�u bilgisayar�mdaki dosya yolu 
            string fileContent = File.ReadAllText(filePath);
            _tokens = fileContent.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            textBox1.Text = fileContent; // Metni TextBox'a ata
        }

        int i = 0; // Belirli ko�ullar� takip etmek i�in saya�

        private void Tokenize()
        {
            foreach (string token in _tokens)
            {
                string trimmedToken = token.Trim();

                // ; ile bitip 1 keyword tan�mlanm��sa
                if (trimmedToken.EndsWith(";") && i == 1)
                {
                    // ; den �nceki de�er ile ikisini b�lme i�lemi yap.
                    string literalPart = trimmedToken.Substring(0, trimmedToken.Length - 1).Trim();
                    if (!string.IsNullOrEmpty(literalPart))
                    {
                        _tokenList.Add(new Token { Type = TokenType.Literal, Value = literalPart });
                    }
                    _tokenList.Add(new Token { Type = TokenType.Semicolon, Value = ";" });
                    if (IsNext(token))
                    {
                        i = 0; // ;den sonra ba�ka bir de�i�ken tan�mlan�rsa sayac�n s�f�rlanmas� gerekiyor o y�zden yazmak gerekli.
                    }
                }
                else if (i > 1)
                {
                    listBox2.Items.Add(" Hata! Birden fazla anahtar kelime tan�mland�..."); // Birden fazla anahtar kelime bir ; den �nce tan�mlan�rsa hata mesaj� g�ster
                    _tokenList.Clear(); // Token listesini temizle
                    break; //d�ng�den ��kmal� bir daha girmeyecek...
                }
                else
                {
                    // Bo�luk karakterine g�re tokenlar� ay�r
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
                                _tokenList.Clear(); // E�er 2.kez buraya girerse int int gibi 2 keyword tan�m� yap�lm��t�r hata vermesi gerekiyor daha �nceki ald��� verileri silmesi i�in yazd�m.
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

        // Bir token�n say�sal bir de�er olup olmad���n� kontrol et
        private bool IsNumericLiteral(string token)
        {
            double result;
            return double.TryParse(token, out result);
        }

        // Bir token�n anahtar kelime olup olmad���n� kontrol et
        private bool IsKeyword(string token)
        {
            const string keywordPattern = @"int|string|float|double";
            return Regex.IsMatch(token, keywordPattern);
        }

        // Bir token�n ge�erli bir tan�mlay�c� olup olmad���n� kontrol et
        private bool IsIdentifier(string token)
        {
            string pattern = @"[a-zA-Z_][a-zA-Z_0-9]*";
            return Regex.IsMatch(token, pattern);
        }

        // Bir token�n operat�r olup olmad���n� kontrol et
        private bool IsOperator(string token)
        {
            const string operatorPattern = @"[=+\-*/%]";
            return Regex.IsMatch(token, operatorPattern);
        }

        // ; den sonraki tokende keyword geliyorsa yazd�rmaya devam etmesi i�in ekledim. Eklemedi�im takdirde birden fazla keyword kulland�n�z diyip veriyi okumuyordu.
        private bool IsNext(string token)
        {
            const string pattern = @";";
            return Regex.IsMatch(token, pattern);
        }

        // Tokenlardan ifadeleri ��z�mle
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

        // Buton t�klama olay�, tokenle�tirme ve de�erlendirme i�lemini ba�lat�r
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear(); // �fadeler i�in listbox'� temizle
            listBox2.Items.Clear(); // Tokenlar i�in listbox'� temizle
            _expressionList.Clear(); // �nceki ifadeleri temizle
            Tokenize(); // Girdiyi tokenle
            EvaluateExpressions(); // Tokenlardan ifadeleri ��z�mle
            DisplayTokens(); // Tokenlar� listbox'ta g�ster
            DisplayExpressions(); // �fadeleri listbox'ta g�ster
        }

        // Tokenlar� listbox'ta g�ster
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

        // �fadeleri listbox'ta g�ster
        private void DisplayExpressions()
        {
            listBox1.Items.Clear(); // �fadeleri eklemeden �nce listbox'� temizle
            foreach (Expression expression in _expressionList)
            {
                listBox1.Items.Add($"{expression.LeftOperand} {expression.Operator} {expression.RightOperand}");
            }
        }

        // Token s�n�f�, bir token� temsil eder
        public class Token
        {
            public TokenType Type { get; set; }
            public string Value { get; set; }
        }

        // �fade s�n�f�, bir ifadeyi temsil eder
        public class Expression
        {
            public string LeftOperand { get; set; }
            public string RightOperand { get; set; }
            public string Operator { get; set; }
        }

        // Token t�rleri i�in enum
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