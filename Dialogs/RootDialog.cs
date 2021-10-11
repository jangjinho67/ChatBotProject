using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Xml;
using System.Collections.Generic;

namespace Project
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        Random random = new Random();
        List<string> user = new List<string>();
        List<string> computer = new List<string>();
        string link = "https://stdict.korean.go.kr/api/search.do?certkey_no=2356&key=";
        string key = "9717EC333DE82602C2A671DA6CBFF851";
        string rest = "&type_search=search&q=";
        string data = "";
        char temp;
        char comtemp;
        int count = 3;
        int total = 0;
        string urlString = "";
        void inputComputer(char data)
        {
            rest = "&type_search=search&advanced=y&method=start&pos=1&letter_s=2&q=";
            urlString = link + key + rest + data;
            XmlDocument doc = new XmlDocument();
            doc.Load(urlString);
            XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
            int total = Int32.Parse(ltotal.InnerText);
            if (total == 0)
            {
                //await context.PostAsync("No more words you win!");
                //System.Environment.Exit(-1);
            }
            else
            {
                while (true)
                {
                    string temp = isYes(data);
                    // 사용자가 입력하지 않았으나 컴퓨터가 같은 값을 입력한 경우
                    if (computer.Contains(temp))
                    {
                        continue;
                    }
                    // 컴퓨터는 입력하지 않았으나 사용자가 입력한 경우
                    else if (user.Contains(temp))
                    {
                        continue;
                    }
                    // 컴퓨터도 사용자도 입력하지 않은 경우
                    else
                    {
                        computer.Add(temp);
                        break;
                    }
                }
            }
        }
        string isYes(char data)
        {
            rest = "&type_search=search&advanced=y&method=start&pos=1&letter_s=2&q=";
            string temp = "";
            urlString = link + key + rest + data;
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(urlString);
            XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
            int total = Int32.Parse(ltotal.InnerText);
            if (total <= 10)
            {
                int specific = random.Next(0, total);
                XmlNode word = doc.SelectNodes("//channel/item/word")[specific];
                temp = word.InnerText;
            }
            else
            {
                // 최대 검색 갯수가 100개 이므로 초과시 100개로 한정
                if (total > 100)
                    total = 100;
                int specific = random.Next(0, total);
                urlString = link + key + "&num=" + total.ToString() + rest + data;
                doc.Load(urlString);
                // 최대 100개의 검색 결과 중 임의의 단어 추출
                XmlNode word = doc.SelectNodes("//channel/item/word")[specific];
                temp = word.InnerText;
            }
            return temp;
        }
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageChoiceAsync);
        }

        public async Task MessageChoiceAsync(IDialogContext context,
                                               IAwaitable<object> result)
        {
            var message = context.MakeMessage();
            var actions = new List<CardAction>();

            actions.Add(new CardAction() { Title = "1. 게임 설명", Value = "1", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "2. 게임 시작", Value = "2", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "3. 종료", Value = "3", Type = ActionTypes.ImBack });

            message.Attachments.Add(
                new HeroCard { Title = "끝말잇기", Buttons = actions }.ToAttachment()
            );

            await context.PostAsync(message);
            context.Wait(SendMessageAsync);
        }

        public async Task SendMessageAsync(IDialogContext context,
                                               IAwaitable<object> result)
        {
            Activity activity = await result as Activity;
            string strSelected = activity.Text.Trim();

            if (strSelected == "1")
            {
                await context.PostAsync("게임 설명");
                context.Wait(MessageChoiceAsync);
            }
            else if (strSelected == "2")
            {
                await context.PostAsync("게임을 시작합니다. 먼저 단어를 입력해주세요.");
                context.Wait(MessageReceivedAsync);
            }
            else if (strSelected == "3")
            {
                await context.PostAsync("게임 종료");
                context.Done(1);
            }
            else
            {
                await context.PostAsync("잘못 입력");
                context.Wait(MessageChoiceAsync);
            }
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var input = await argument;
            data = input.Text;
            // 첫 실행시 (컴퓨터 리스트에 아무 값도 없음)
            if (computer.Count == 0)
            {
                // 단어 유무 판별
                urlString = link + key + rest + data;
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.Load(urlString);
                XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
                total = Int32.Parse(ltotal.InnerText);
                // 단어가 없을 때
                if (total == 0)
                {
                    count = count - 1;
                    await context.PostAsync($"존재하지 않는 단어입니다, {count}번의 기회가 남았습니다.");
                }
                // 단어가 있을 때
                else
                {
                    temp = data[data.Length - 1];
                    count = 3;
                    // 리스트에 단어 추가
                    user.Add(data);
                    // 끝값 비교
                    inputComputer(temp);
                    await context.PostAsync($"{computer[computer.Count - 1]}");
                }
            }
            // 첫 실행이 아닐 때 (컴퓨터 리스트에 단어가 들어가 있음)
            else
            {
                urlString = link + key + rest + data;
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.Load(urlString);
                XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
                total = Int32.Parse(ltotal.InnerText);
                // 단어가 없을 때
                if (total == 0)
                {
                    count = count - 1;
                    await context.PostAsync($"존재하지 않는 단어입니다, {count}번의 기회가 남았습니다.");
                }
                // 단어가 있을 때
                else
                {
                    temp = data[0];
                    comtemp = computer[computer.Count - 1][computer[computer.Count - 1].Length - 1];
                    if (temp != comtemp)
                    {
                        count = count - 1;
                        await context.PostAsync($"마지막 글자가 서로 일치하지 않습니다. {count}번의 기회가 남았습니다.");
                    }
                    else
                    {
                        urlString = link + key + rest + data;
                        // 한 번만 불러옴 (값 유무 여부만 빠르게 반환)
                        doc.PreserveWhitespace = false;
                        doc.Load(urlString);
                        ltotal = doc.SelectNodes("//channel/total")[0];
                        total = Int32.Parse(ltotal.InnerText);
                        // 검색 결과가 없는 경우
                        if (total == 0)
                        {
                            count = count - 1;
                            await context.PostAsync($"존재하지 않는 단어입니다, {count}번의 기회가 남았습니다.");
                        }
                        // 검색 결과가 있는 경우
                        else
                        {
                            // 사용자로부터 이미 입력 받은 적이 있는 경우
                            if (user.Contains(data))
                            {
                                count = count - 1;
                                await context.PostAsync($"이미 사용된 단어입니다. {count}번의 기회가 남았습니다.");
                            }
                            // 사용자로부터 입력을 받지 않았으나 컴퓨터가 입력한 경우
                            else if (computer.Contains(data))
                            {
                                count = count - 1;
                                await context.PostAsync($"이미 사용된 단어입니다. {count}번의 기회가 남았습니다.");
                            }
                            // 둘 다 사용하지 않은 첫번째 단어인 경우 사용자 리스트에 저장
                            else
                            {
                                temp = data[data.Length - 1];
                                count = 3;
                                user.Add(data);
                                inputComputer(temp);
                                await context.PostAsync($"{computer[computer.Count - 1]}");
                            }
                        }
                    }
                }
            }
            context.Wait(MessageReceivedAsync);
            if (count == 0)
            {
                await context.PostAsync("남은 기회가 없으므로 게임이 종료되었습니다");
                await context.PostAsync("게임을 다시 하려면 아무키나 눌러주세요");
                context.Done(1);
            }
        }
    }
}