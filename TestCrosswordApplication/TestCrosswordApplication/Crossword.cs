using System;
using System.Collections.Generic;


namespace TestCrosswordApplication
{/// <summary>
/// class for crossword logic
/// </summary>
    public class Crossword
    {
        List<string> words = new List<string>();// для всех слов

        List<string> nomatchWords = new List<string>();//для непересекающихся слов

        List<string> usedWords = new List<string>();//для уже использованных слов

        char[,] renderResult;//результат как сетка слов
        //сетка.
        //3 значения для int[]:
        //[0]==горизонтально/вертикально расположенное слово(1/0)
        //[1]==начало (номер первой буквы для горизонтальной оси)
        //[2]==beginning(номер первой буквы для вертикальной оси)
        List<int[]> Grid = new List<int[]>();


        int HorizontalSize = 11;//вообще тут надо проверять и, возможно, расширять границы, так как можно наткнуться на indexOutOfRangeException при генерации  
        int VerticalSize = 11;

        public Crossword()
        {
            FillWords(this.words);
        }

        void FillWords(List<string> words)// заполнение списка словами (можно сделать чтение из файла, например)
        {
            words.Add("перелом");
            words.Add("подвывих");
            words.Add("закрытый");
            words.Add("ушиб");
            words.Add("открытый");
            words.Add("вина");
            words.Add("вывих");
        }

        /// <summary>
        /// the main method. Here we render all words in words-list.
        /// </summary>
        /// <param name="crossword"></param>
        public void Generate(Crossword crossword) //DatagridView dataGridView как второй параметр для вывода в WinForms или WPF
        {
            List<int[]> letterPosition = new List<int[]>();//может быть пустой поскольку слова не пересекаются
                                                           //помещает каждое слово в кроссворд
            while (words.Count != 0)//если words.Count=0, все слова размещены (но также могут быть слова без соответствия)
            {
                string currentWord = words[0];
                Grid.Add(new int[3] { 1, 0, 0 });//новая позиция в сетке для слова
                if (usedWords.Count == 0)//это первое слово
                {
                    usedWords.Add(currentWord);
                    crossword.Place(currentWord, Grid[0], letterPosition, 0);//
                    words.RemoveAt(0);
                }
                else//не первое слово => помещает новое слово в положение относительно любого из используемых слов 
                    SolveCrossword(currentWord, letterPosition, 0);
                PrintCrossword();//метод вывода кроссворда 
            }
            //шаг без совпадающих слов. Слова которые могут пересекаться с другими словами корректно, отобразятся в кроссворде.
            foreach (string currentWord in nomatchWords)
                SolveCrossword(currentWord, letterPosition, 0);
        }


        /// <summary>
        /// solution of crossword
        /// </summary>
        /// <param name="currentWord">the word that we try to place</param>
        /// <param name="letterPosition">positions of letter where current word can intersect with the other</param>
        /// <param name="intersectWord">the number of intersect word</param>
        void SolveCrossword(string currentWord, List<int[]> letterPosition, int intersectWord)
        {
            if (usedWords.Count - intersectWord > 0)//проверка на пересечение с другими словами =>SolveCrossword проверяет пересечение от последнего использованного слова до первого
                                                    //другой порядок приведет к генерации из последних слов => кроссворд будет выглядеть как дерево вместо нужного нам вида пересечений
            {
                try
                {
                    if (MatchWords(usedWords[intersectWord], currentWord, letterPosition))//нет совпадений для данного слова с последним словом
                    {//usedWords.Count - 1 - 
                        int positionCounter = 0;
                        if (this.Place(currentWord, Grid[intersectWord], letterPosition, positionCounter) == false)
                        {
                            letterPosition.Clear();
                            SolveCrossword(currentWord, letterPosition, ++positionCounter);
                        }
                        if (words.Count != 0)
                            words.RemoveAt(0);
                        return;
                    }
                    else
                    {
                        letterPosition.Clear();
                        SolveCrossword(currentWord, letterPosition, intersectWord + 1);
                    }


                    //если мы тута и пересечение не было найдено=>это не совпадающее слово. Оно может быть правильным или полностью несоответствующим, мы выясним это на последнем шаге
                    nomatchWords.Add(currentWord);
                    words.RemoveAt(0);
                }
                catch//будет исключение если номер непроверенной позиции->0
                {
                    letterPosition.Clear();
                    SolveCrossword(currentWord, letterPosition, intersectWord + 1);
                }
            }
        }

        //ищем совпадения
        static bool MatchWords(string newword, string matchedword, List<int[]> letterPosition)
        {
            for (int i = 0; i < newword.Length; i++)
                for (int j = 0; j < matchedword.Length; j++)
                    if (newword[i] == matchedword[j])
                        letterPosition.Add(new int[2] { i, j });//нам нужны все совпадения, а не только первое<= могут быть проблемы с размером кроссворда и большим количеством несоответствующих слов
            if (letterPosition.Count != 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// This is how to place words into crossword grid
        /// </summary>
        /// <param name="newword">our word</param>
        /// <param name="letterPosition">Letter Positions list</param>
        /// <param name="el">the number of element of LetterPositions. If current intersection is not valid, we move to next</param>
        /// <returns>true if sucсess</returns>
        bool Place(string newword, int[] intersectedWordGrid, List<int[]> letterPosition, int el)//
        {
            //размещение нового слова, связанного с последним словом(установка последнего элемента Grid)
          
            if (Grid.Count > 1)
            {
                if (intersectedWordGrid[0] == 1)//последнее слово горизонтально=>нам нужно вертикально
                {
                    Grid[Grid.Count - 1][0] = 0;
                    Grid[Grid.Count - 1][1] = intersectedWordGrid[1] - letterPosition[el][1];
                    Grid[Grid.Count - 1][2] = intersectedWordGrid[2] + letterPosition[el][0];
                }
                else//вертикально=>горизонтально
                {
                    Grid[Grid.Count - 1][0] = 1;
                    Grid[Grid.Count - 1][1] = intersectedWordGrid[1] + letterPosition[el][0];
                    Grid[Grid.Count - 1][2] = intersectedWordGrid[2] - letterPosition[el][1];
                }
                usedWords.Add(newword);
            }

            // позиция слов соответствует Grid=>ок
            if (Render())
            {
                letterPosition.Clear();
                return true;
            }
            else//кроссворд не работает с сеткой 
                Place(newword, intersectedWordGrid, letterPosition, ++el);

            return true;
        }

        //рендеринг кроссворда
        bool Render()
        {
            if (renderResult == null)
                renderResult = new char[HorizontalSize, VerticalSize];
            int[] renderword = Grid[Grid.Count - 1];//для каждых координат Grid 
            {
                if (usedWords.Count != 0)
                {
                    string currentword = usedWords[usedWords.Count - 1];
                    //usedWords.RemoveAt(0);
                    int i = 0, j = 0;//нужен i & j потому что изменение координат сетки может вызвать ошибки в других позициях слов
                    try
                    {
                        //rresultTempCopy = renderRes;
                        if (CheckWordsInSolution(currentword, renderResult, renderword))
                            for (int k = 0; k < currentword.Length; k++)//слово прошло проверку=>записываем его
                            {
                                renderResult[HorizontalSize / 2 + renderword[1] + i, VerticalSize / 2 + renderword[2] + j] = currentword[k];
                                if (renderword[0] == 1)
                                    j++;
                                else
                                    i++;
                            }
                        else
                        {
                            usedWords.RemoveAt(usedWords.Count - 1);
                            return false;
                        }
                    }
                    catch
                    {
                        usedWords.RemoveAt(usedWords.Count - 1);
                        return false;
                    }
                }
            }
            return true;//все ок
        }

        bool CheckWordsInSolution(string currentword, char[,] solutionTable, int[] wordCoordinates)
        {
            int i = 0, j = 0;
            for (int k = 0; k < currentword.Length; k++)
                //валидация
                if ((renderResult[HorizontalSize / 2 + wordCoordinates[1] + i, VerticalSize / 2 + wordCoordinates[2] + j] == '\0' ||
                        renderResult[HorizontalSize / 2 + wordCoordinates[1] + i, VerticalSize / 2 + wordCoordinates[2] + j] == currentword[k])
                                                                                      
                    )
                {
                    if (wordCoordinates[0] == 1)
                        j++;
                    else
                        i++;
                }
                else
                    return false;
            return true;
        }

        void PrintCrossword()
        {
            //сюда рисуем кроссворд
        }
    }
}
