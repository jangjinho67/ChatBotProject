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
                    // ����ڰ� �Է����� �ʾ����� ��ǻ�Ͱ� ���� ���� �Է��� ���
                    if (computer.Contains(temp))
                    {
                        continue;
                    }
                    // ��ǻ�ʹ� �Է����� �ʾ����� ����ڰ� �Է��� ���
                    else if (user.Contains(temp))
                    {
                        continue;
                    }
                    // ��ǻ�͵� ����ڵ� �Է����� ���� ���
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
                // �ִ� �˻� ������ 100�� �̹Ƿ� �ʰ��� 100���� ����
                if (total > 100)
                    total = 100;
                int specific = random.Next(0, total);
                urlString = link + key + "&num=" + total.ToString() + rest + data;
                doc.Load(urlString);
                // �ִ� 100���� �˻� ��� �� ������ �ܾ� ����
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

            actions.Add(new CardAction() { Title = "1. ���� ����", Value = "1", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "2. ���� ����", Value = "2", Type = ActionTypes.ImBack });
            actions.Add(new CardAction() { Title = "3. ����", Value = "3", Type = ActionTypes.ImBack });

            message.Attachments.Add(
                new HeroCard { Title = "�����ձ�", Buttons = actions }.ToAttachment()
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
                await context.PostAsync("���� ����");
                context.Wait(MessageChoiceAsync);
            }
            else if (strSelected == "2")
            {
                await context.PostAsync("������ �����մϴ�. ���� �ܾ �Է����ּ���.");
                context.Wait(MessageReceivedAsync);
            }
            else if (strSelected == "3")
            {
                await context.PostAsync("���� ����");
                context.Done(1);
            }
            else
            {
                await context.PostAsync("�߸� �Է�");
                context.Wait(MessageChoiceAsync);
            }
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var input = await argument;
            data = input.Text;
            // ù ����� (��ǻ�� ����Ʈ�� �ƹ� ���� ����)
            if (computer.Count == 0)
            {
                // �ܾ� ���� �Ǻ�
                urlString = link + key + rest + data;
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.Load(urlString);
                XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
                total = Int32.Parse(ltotal.InnerText);
                // �ܾ ���� ��
                if (total == 0)
                {
                    count = count - 1;
                    await context.PostAsync($"�������� �ʴ� �ܾ��Դϴ�, {count}���� ��ȸ�� ���ҽ��ϴ�.");
                }
                // �ܾ ���� ��
                else
                {
                    temp = data[data.Length - 1];
                    count = 3;
                    // ����Ʈ�� �ܾ� �߰�
                    user.Add(data);
                    // ���� ��
                    inputComputer(temp);
                    await context.PostAsync($"{computer[computer.Count - 1]}");
                }
            }
            // ù ������ �ƴ� �� (��ǻ�� ����Ʈ�� �ܾ �� ����)
            else
            {
                urlString = link + key + rest + data;
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.Load(urlString);
                XmlNode ltotal = doc.SelectNodes("//channel/total")[0];
                total = Int32.Parse(ltotal.InnerText);
                // �ܾ ���� ��
                if (total == 0)
                {
                    count = count - 1;
                    await context.PostAsync($"�������� �ʴ� �ܾ��Դϴ�, {count}���� ��ȸ�� ���ҽ��ϴ�.");
                }
                // �ܾ ���� ��
                else
                {
                    temp = data[0];
                    comtemp = computer[computer.Count - 1][computer[computer.Count - 1].Length - 1];
                    if (temp != comtemp)
                    {
                        count = count - 1;
                        await context.PostAsync($"������ ���ڰ� ���� ��ġ���� �ʽ��ϴ�. {count}���� ��ȸ�� ���ҽ��ϴ�.");
                    }
                    else
                    {
                        urlString = link + key + rest + data;
                        // �� ���� �ҷ��� (�� ���� ���θ� ������ ��ȯ)
                        doc.PreserveWhitespace = false;
                        doc.Load(urlString);
                        ltotal = doc.SelectNodes("//channel/total")[0];
                        total = Int32.Parse(ltotal.InnerText);
                        // �˻� ����� ���� ���
                        if (total == 0)
                        {
                            count = count - 1;
                            await context.PostAsync($"�������� �ʴ� �ܾ��Դϴ�, {count}���� ��ȸ�� ���ҽ��ϴ�.");
                        }
                        // �˻� ����� �ִ� ���
                        else
                        {
                            // ����ڷκ��� �̹� �Է� ���� ���� �ִ� ���
                            if (user.Contains(data))
                            {
                                count = count - 1;
                                await context.PostAsync($"�̹� ���� �ܾ��Դϴ�. {count}���� ��ȸ�� ���ҽ��ϴ�.");
                            }
                            // ����ڷκ��� �Է��� ���� �ʾ����� ��ǻ�Ͱ� �Է��� ���
                            else if (computer.Contains(data))
                            {
                                count = count - 1;
                                await context.PostAsync($"�̹� ���� �ܾ��Դϴ�. {count}���� ��ȸ�� ���ҽ��ϴ�.");
                            }
                            // �� �� ������� ���� ù��° �ܾ��� ��� ����� ����Ʈ�� ����
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
                await context.PostAsync("���� ��ȸ�� �����Ƿ� ������ ����Ǿ����ϴ�");
                await context.PostAsync("������ �ٽ� �Ϸ��� �ƹ�Ű�� �����ּ���");
                context.Done(1);
            }
        }
    }
}