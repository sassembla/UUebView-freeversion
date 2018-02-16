using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UUebView
{
    public class TMProPlugin : IPluggable
    {
        private static TMPro.TextMeshProUGUI tmGoComponent;
        private readonly DefaultBehaviour defaultBehaviour;

        public TMProPlugin()
        {
            defaultBehaviour = new DefaultBehaviour();
        }

        IEnumerator<ChildPos> IPluggable.TextLayoutCoroutine(Component sourceComponent, TagTree textTree, string text, ViewCursor textViewCursor, Func<InsertType, TagTree, ViewCursor> insertion = null)
        {
            if (sourceComponent is Text)
            {
                return defaultBehaviour.DoTextComponentLayout(textTree, (Text)sourceComponent, text, textViewCursor, insertion);
            }
            if (sourceComponent is TMPro.TextMeshProUGUI)
            {
                return DoTextMeshProComponentLayout(textTree, (TMPro.TextMeshProUGUI)sourceComponent, text, textViewCursor, insertion);
            }
            throw new Exception("component not supported:" + sourceComponent);
        }

        /**
            TextMesh Proのレイアウトを決定して返す
            レイアウト、改行などの必要に応じて文字列を分割する。
         */
        private IEnumerator<ChildPos> DoTextMeshProComponentLayout(TagTree textTree, TMPro.TextMeshProUGUI textComponent, string text, ViewCursor textViewCursor, Func<InsertType, TagTree, ViewCursor> insertion = null)
        {
            // Debug.Log("DoTextMeshProComponentLayout text:" + text.Length + " textViewCursor:" + textViewCursor);
            textComponent.text = text;

            // textComponentに対してwidthをセットする必要がある。
            textComponent.rectTransform.sizeDelta = new Vector2(textViewCursor.viewWidth, float.PositiveInfinity);

            // このメソッドは、コンポーネントがgoにアタッチされてcanvasに乗っている場合のみ動作する。
            var textInfos = textComponent.GetTextInfo(text);

            // 各行の要素とパラメータを取得する。
            var tmGeneratorLines = textInfos.lineInfo;
            var lineSpacing = textComponent.lineSpacing;
            var tmLineCount = textInfos.lineCount;


            var onLayoutPresetX = (float)textTree.keyValueStore[HTMLAttribute._ONLAYOUT_PRESET_X];
            // Debug.Log("text:" + text + " textViewCursor.viewWidth:" + textViewCursor.viewWidth);

            // 1行以上のラインが画面内にある。

            var isStartAtZeroOffset = onLayoutPresetX == 0 && textViewCursor.offsetX == 0;
            var isMultilined = 1 < tmLineCount;


            // このコンテナの1行目を別のコンテナの結果位置 = 行中から書いた結果、この1行の幅が画面幅を超えている場合、全体を次の行に送る。
            // あ、この判定では無理だな、、分割されたコンテナの可能性が出てくる？ 整列を下からではなく上からやる必要がある。
            if (!isStartAtZeroOffset && textViewCursor.viewWidth < tmGeneratorLines[0].length)
            {
                // 行なかで、1行目のコンテンツがまるきり入らなかった。
                // よって、改行を行なって次の行からコンテンツを開始する。
                // textTree.keyValueStore[HTMLAttribute._ONLAYOUT_PRESET_X] = 0.0f;
                insertion(InsertType.RetryWithNextLine, null);

                // テキストとサイズを空に戻す
                textComponent.text = string.Empty;
                textComponent.rectTransform.sizeDelta = Vector2.zero;

                yield break;
            }

            // 複数行存在するんだけど、2行目のスタートが0文字目の場合、1行目に1文字も入っていない。
            if (isMultilined && tmGeneratorLines[1].firstCharacterIndex == 0)
            {
                // 行頭でこれが起きる場合、コンテンツ幅が圧倒的に不足していて、一文字も入らないということが起きている。
                // 1文字ずつ切り分けて表示する。
                if (isStartAtZeroOffset)
                {
                    // 最初の1文字目を強制的にセットする
                    var bodyContent = text.Substring(0, 1);

                    // 内容の反映
                    textTree.keyValueStore[HTMLAttribute._CONTENT] = bodyContent;

                    // 最終行
                    var lastLineContent = text.Substring(1);

                    // 最終行を分割して送り出す。追加されたコンテンツを改行後に処理する。
                    var nextLineContent = new InsertedTree(textTree, lastLineContent, textTree.tagValue);
                    insertion(InsertType.InsertContentToNextLine, nextLineContent);


                    var charHeight = (tmGeneratorLines[0].lineHeight + lineSpacing);

                    // テキストとサイズを空に戻す
                    textComponent.text = string.Empty;
                    textComponent.rectTransform.sizeDelta = Vector2.zero;


                    yield return textTree.SetPos(textViewCursor.offsetX, textViewCursor.offsetY, textViewCursor.viewWidth, charHeight);
                    yield break;
                }

                // 行中からのコンテンツ追加で、複数行があるので、コンテンツ全体を次の行で開始させる。
                insertion(InsertType.RetryWithNextLine, null);

                // テキストとサイズを空に戻す
                textComponent.text = string.Empty;
                textComponent.rectTransform.sizeDelta = Vector2.zero;

                yield break;
            }

            if (isStartAtZeroOffset)
            {
                if (isMultilined)
                {
                    // Debug.LogError("行頭での折り返しのある複数行 text:" + text + " textViewCursor.offsetX:" + textViewCursor.offsetX + " tmLineCount:" + tmLineCount);
                    /*
                        TMProのtextInfo上のレイアウト指示と、実際にレイアウトした時に自動的に分割されるワードに差がある。
                        abc が a\nbcになることもあれば、レイアウト時には分割されずabcで入ってしまうこともある。
                        これは予知できないので、textInfoでの分割を正にする方向で対処する。
                        具体的に言うと、文章に人力で\nを入れる。
                     */

                    var bodyContent = string.Empty;
                    var lastLineContent = string.Empty;

                    // TMProの場合、レイアウト時に文字を改行する場所と、実際にコンテンツを放り込んでしまって改行される箇所にズレがある。
                    // よってこの時点で、改行を含んだ文字列へと強制的に変更する。
                    for (var i = 0; i < tmLineCount; i++)
                    {
                        var lineInfo = tmGeneratorLines[i];
                        var lineText = text.Substring(lineInfo.firstCharacterIndex, lineInfo.lastCharacterIndex - lineInfo.firstCharacterIndex + 1);
                        if (i == tmLineCount - 1)
                        {
                            lastLineContent = lineText;
                            continue;
                        }

                        bodyContent += lineText;// + "\n"
                    }


                    // 内容の反映
                    textTree.keyValueStore[HTMLAttribute._CONTENT] = bodyContent;

                    // 最終行を分割して送り出す。追加されたコンテンツを改行後に処理する。
                    var nextLineContent = new InsertedTree(textTree, lastLineContent, textTree.tagValue);
                    insertion(InsertType.InsertContentToNextLine, nextLineContent);


                    // 最終行以外はハコ型に収まった状態なので、ハコとして出力する。
                    // 最終一つ前までの高さを出して、このコンテンツの高さとして扱う。
                    var totalHeight = 0f;
                    for (var i = 0; i < tmLineCount - 1; i++)
                    {
                        var line = tmGeneratorLines[i];
                        totalHeight += (line.lineHeight + lineSpacing);
                    }

                    // テキストとサイズを空に戻す
                    textComponent.text = string.Empty;
                    textComponent.rectTransform.sizeDelta = Vector2.zero;

                    // このビューのポジションをセット
                    yield return textTree.SetPos(textViewCursor.offsetX, textViewCursor.offsetY, textViewCursor.viewWidth, totalHeight);
                }
                else
                {
                    // Debug.LogError("行頭の単一行 text:" + text);
                    var currentLineWidth = textComponent.preferredWidth;
                    var currentLineHeight = (tmGeneratorLines[0].lineHeight + lineSpacing);

                    // 最終行かどうかの判断はここではできないので、単一行の入力が終わったことを親コンテナへと通知する。
                    insertion(InsertType.TailInsertedToLine, textTree);

                    var childPos = textTree.SetPos(textViewCursor.offsetX, textViewCursor.offsetY, currentLineWidth, currentLineHeight);

                    // テキストとサイズを空に戻す
                    textComponent.text = string.Empty;
                    textComponent.rectTransform.sizeDelta = Vector2.zero;

                    yield return childPos;
                }
            }
            else
            {
                if (isMultilined)
                {
                    // Debug.LogError("行中追加での折り返しのある複数行 text:" + text);
                    var currentLineHeight = (tmGeneratorLines[0].lineHeight + lineSpacing);

                    // 複数行が途中から出ている状態で、まず折り返しているところまでを分離して、後続の文章を新規にstringとしてinsertする。
                    var currentLineContent = text.Substring(0, tmGeneratorLines[1].firstCharacterIndex);
                    textTree.keyValueStore[HTMLAttribute._CONTENT] = currentLineContent;

                    // get preferredWidht of text from trimmed line.
                    textComponent.text = currentLineContent;

                    var currentLineWidth = textComponent.preferredWidth;

                    var restContent = text.Substring(tmGeneratorLines[1].firstCharacterIndex);
                    var nextLineContent = new InsertedTree(textTree, restContent, textTree.tagValue);

                    // 次のコンテンツを新しい行から開始する。
                    insertion(InsertType.InsertContentToNextLine, nextLineContent);

                    // テキストとサイズを空に戻す
                    textComponent.text = string.Empty;
                    textComponent.rectTransform.sizeDelta = Vector2.zero;

                    yield return textTree.SetPos(textViewCursor.offsetX, textViewCursor.offsetY, currentLineWidth, currentLineHeight);
                }
                else
                {
                    // Debug.LogError("行中追加の単一行 text:" + text);
                    var width = textComponent.preferredWidth;
                    var height = (tmGeneratorLines[0].lineHeight + lineSpacing);

                    // Debug.LogError("行中の単一行 text:" + text + " textViewCursor:" + textViewCursor);
                    // 最終行かどうかの判断はここでできないので、単一行の入力が終わったことを親コンテナへと通知する。
                    insertion(InsertType.TailInsertedToLine, textTree);

                    // テキストとサイズを空に戻す
                    textComponent.text = string.Empty;
                    textComponent.rectTransform.sizeDelta = Vector2.zero;

                    yield return textTree.SetPos(textViewCursor.offsetX, textViewCursor.offsetY, width, height);
                }
            }
        }

        float IPluggable.GetDefaultHeightOfContainerText(Component textComponentSrc)
        {
            if (textComponentSrc is Text)
            {
                return defaultBehaviour.GetDefaultHeightOfContainerText(textComponentSrc);
            }
            else
            {
                if (textComponentSrc is TMPro.TextMeshProUGUI)
                {
                    var textComponent = (TMPro.TextMeshProUGUI)textComponentSrc;
                    textComponent.text = "A";

                    var defaultHeight = textComponent.preferredHeight;
                    textComponent.text = string.Empty;

                    return defaultHeight;
                }
            }

            return -1;
        }

        Component IPluggable.TextComponent(GameObject prefab, string uuebTagsName)
        {
            var textComponent = prefab.GetComponent<Text>();
            if (textComponent != null)
            {
                if (textComponent.font == null)
                {
                    throw new Exception("font is null. prefab:" + uuebTagsName + "/" + prefab.name);
                }
                return textComponent;
            }
            else
            {
                if (tmGoComponent == null)
                {
                    var targetCanvasArray = GameObject.FindObjectsOfType<Canvas>();
                    if (targetCanvasArray == null || targetCanvasArray.Length == 0)
                    {
                        throw new Exception("UUebView with TMPro requires at least 1 visible canvas.");
                    }

                    Canvas targetCanvas = null;
                    foreach (var canvas in targetCanvasArray)
                    {
                        if (canvas.isActiveAndEnabled)
                        {
                            targetCanvas = canvas;
                            break;
                        }
                    }

                    var tmGo = GameObject.Instantiate(prefab, targetCanvas.transform);// 必須
                    tmGoComponent = tmGo.GetComponent<TMPro.TextMeshProUGUI>();
                    tmGoComponent.text = string.Empty;
                }
                else
                {
                    var t = prefab.GetComponent<TMPro.TextMeshProUGUI>();

                    // レイアウトだけだったらこれらのパラメータで足りる、みたいなのを集めて使おう。
                    tmGoComponent.font = t.font;
                    tmGoComponent.fontSize = t.fontSize;
                    tmGoComponent.fontStyle = t.fontStyle;

                    tmGoComponent.text = string.Empty;
                }

                if (tmGoComponent.font == null)
                {
                    throw new Exception("font is null. prefab:" + uuebTagsName + "/" + prefab.name);
                }

                return tmGoComponent;
            }
        }

        void IPluggable.SetText(GameObject targetGameObject, string text)
        {
            var textComponent = targetGameObject.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
            else
            {
                var textComponentPro = targetGameObject.GetComponent<TMPro.TextMeshProUGUI>();
                textComponentPro.text = text;
            }
        }
    }
}