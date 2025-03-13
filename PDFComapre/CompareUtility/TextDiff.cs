using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareUtility
{
    public class TextDiff
    {
        private StringBuilder _content;

        private StringBuilder _source;
        private StringBuilder _target;

        private StringBuilder _sourceSentence;
        private StringBuilder _targetSentence;

        private StringBuilder _sourceWord;
        private StringBuilder _targetWord;

        public string CompareText(List<string> source, List<string> target, string sourcePath = "", string tagetPath = "")
        {
            try
            {
                _content = new StringBuilder();
                _source = new StringBuilder();
                _target = new StringBuilder();

                _content.Append("<style type='text/css'>.divLeft {width:49.5%;display:block;float: left;border: double;overflow:auto;}");
                _content.Append(".divRight {width:49.5%;display:block;float: right;border: double;overflow:auto;}</style>");

                _source.Append("<div class ='divLeft'>");

                _source.Append("<div style='text-align: center;border:double;'> Source - ");
                _source.Append(sourcePath);
                _source.Append("</br></br></br></div>");

                _target.Append("<div class='divRight'>");
                _target.Append("<div style='text-align: center;border:double;'> Target - ");
                _target.Append(tagetPath);
                _target.Append("</br></br></br></div>");

                if (source.Count > target.Count)
                {
                    for (int i = 0; i < target.Count; i++)
                    {
                        compareSentences(source[i], target[i]);
                        _source.Append(_sourceSentence);
                        _source.Append("</br>");
                        _target.Append(_targetSentence);
                        _target.Append("</br>");
                    }

                    for (int i = target.Count; i < source.Count; i++)
                    {
                        _source.Append("<font color='red'>");
                        //_source.Append(_sourceSentence);
                        _source.Append(source[i]);
                        _source.Append("</font>");
                        _source.Append("</br>");

                    }
                }
                else
                {
                    for (int i = 0; i < source.Count; i++)
                    {
                        compareSentences(source[i], target[i]);
                        _source.Append(_sourceSentence);
                        _source.Append("</br>");
                        _target.Append(_targetSentence);
                        _target.Append("</br>");
                    }

                    for (int i = source.Count; i < target.Count; i++)
                    {
                        _target.Append("<font color='red'>");
                        // _target.Append(_targetSentence);
                        _target.Append(target[i]);
                        _target.Append("</font>");
                        _target.Append("</br>");

                    }
                }
                _content.Append(_source.ToString());
                _content.Append("</br>");
                _content.Append("</br>");
                _content.Append("</div>");

                _content.Append(_target.ToString());
                _content.Append("</br>");
                _content.Append("</br>");
                _content.Append("</div>");

                return _content.ToString();
            }
            catch (Exception ex)
            {
                return "Failure";
            }
        }

        private void compareSentences(string source, string target)
        {
            _sourceSentence = new StringBuilder();
            _targetSentence = new StringBuilder();

            if (source.Trim().Equals(target.Trim()))
            {
                _sourceSentence.Append(source);
                _targetSentence.Append(target);
            }
            else
            {
                List<string> sourceWordlist = source.Split(' ').ToList();
                List<string> targetWordlist = target.Split(' ').ToList();

                sourceWordlist.RemoveAll(x => string.IsNullOrEmpty(x));
                targetWordlist.RemoveAll(x => string.IsNullOrEmpty(x));

                if (sourceWordlist.Count > targetWordlist.Count)
                {
                    for (int i = 0; i < targetWordlist.Count; i++)
                    {
                        compareWord(sourceWordlist[i], targetWordlist[i]);
                        _sourceSentence.Append(" ");
                        _sourceSentence.Append(_sourceWord.ToString());

                        _targetSentence.Append(" ");
                        _targetSentence.Append(_targetWord.ToString());
                    }

                    for (int i = targetWordlist.Count; i < sourceWordlist.Count; i++)
                    {
                        _sourceSentence.Append("<font color='red'>");
                        _sourceSentence.Append(" ");
                        _sourceSentence.Append(sourceWordlist[i]);
                        _sourceSentence.Append("</font>");
                        //_sourceSentence.Append("</br>");

                    }

                    //_sourceSentence.Append("<font color='red'>");
                    //_sourceSentence.Append(source.Substring(targetWordlist.Count, sourceWordlist.Count - targetWordlist.Count));
                    // _sourceSentence.Append("</font>");
                }
                else
                {
                    for (int i = 0; i < sourceWordlist.Count; i++)
                    {
                        compareWord(sourceWordlist[i], targetWordlist[i]);
                        _sourceSentence.Append(" ");
                        _sourceSentence.Append(_sourceWord.ToString());

                        _targetSentence.Append(" ");
                        _targetSentence.Append(_targetWord.ToString());

                    }

                    for (int i = sourceWordlist.Count; i < targetWordlist.Count; i++)
                    {
                        _targetSentence.Append("<font color='red'>");
                        _targetSentence.Append(" ");
                        _targetSentence.Append(targetWordlist[i]);
                        _targetSentence.Append("</font>");
                        //_targetSentence.Append("</br>");

                    }

                    //_targetSentence.Append("<font color='red'>");
                    //_targetSentence.Append(target.Substring(sourceWordlist.Count, targetWordlist.Count - sourceWordlist.Count));
                    //_targetSentence.Append("</font>");
                }
            }

        }

        private void compareWord(string wsource, string wtarget)
        {
            _sourceWord = new StringBuilder();
            _targetWord = new StringBuilder();


            if (wsource.Trim().Equals(wtarget.Trim()))
            {
                _sourceWord.Append(wsource);
                _targetWord.Append(wtarget);
            }
            else
            {
                char[] source = wsource.ToCharArray();
                char[] target = wtarget.ToCharArray();

                if (source.Length > target.Length)
                {
                    for (int i = 0; i < target.Length; i++)
                    {
                        if (source[i] == target[i])
                        {
                            _sourceWord.Append(source[i]);
                            _targetWord.Append(target[i]);
                        }
                        else
                        {
                            _sourceWord.Append("<font color='red'>");
                            _sourceWord.Append(source[i]);
                            _sourceWord.Append("</font>");

                            _targetWord.Append("<font color='red'>");
                            _targetWord.Append(target[i]);
                            _targetWord.Append("</font>");

                        }
                    }

                    for (int i = target.Length; i < source.Length; i++)
                    {
                        _sourceWord.Append("<font color='red'>");
                        _sourceWord.Append(source[i]);
                        _sourceWord.Append("</font>");
                       // _sourceWord.Append("</br>");
                    }

                    //_sourceWord.Append("<font color='red'>");
                    // _sourceWord.Append(wsource.Substring(target.Length, source.Length - target.Length));
                    //_sourceWord.Append("</font>");
                }
                else
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        if (source[i] == target[i])
                        {
                            _sourceWord.Append(source[i]);
                            _targetWord.Append(target[i]);
                        }
                        else
                        {

                            _sourceWord.Append("<font color='red'>");
                            _sourceWord.Append(source[i]);
                            _sourceWord.Append("</font>");

                            _targetWord.Append("<font color='red'>");
                            _targetWord.Append(target[i]);
                            _targetWord.Append("</font>");

                        }
                    }

                    for (int i = source.Length ; i < target.Length; i++)
                    {
                        _targetWord.Append("<font color='red'>");
                        _targetWord.Append(target[i]);
                        _targetWord.Append("</font>");
                       // _targetWord.Append("</br>");
                    }

                    //_targetWord.Append("<font color='red'>");
                    //_targetWord.Append(wtarget.Substring(source.Length, target.Length - source.Length));
                    //_targetWord.Append("</font>");
                }
            }

            
        }
    }
}
