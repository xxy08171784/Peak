#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate AI Course Report PDF."""

import os
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from reportlab.lib.colors import HexColor, grey
from reportlab.platypus import (SimpleDocTemplate, Paragraph, Spacer,
                                 PageBreak, Table, TableStyle)
from reportlab.lib.enums import TA_CENTER, TA_JUSTIFY
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

# Register Chinese font
CN = "Helvetica"
for fp in ["C:/Windows/Fonts/msyh.ttc", "C:/Windows/Fonts/simsun.ttc",
           "C:/Windows/Fonts/simhei.ttf"]:
    if os.path.exists(fp):
        try:
            pdfmetrics.registerFont(TTFont("F", fp))
            CN = "F"
            break
        except Exception:
            pass


def ST(name, **kw):
    return ParagraphStyle(name, fontName=CN, **kw)


# Styles dict for easy access
S = {}
S["ct"] = ST("ct", fontSize=26, leading=36, alignment=TA_CENTER,
              textColor=HexColor("#1a3c6e"), spaceAfter=12*mm)
S["cs"] = ST("cs", fontSize=14, leading=20, alignment=TA_CENTER,
              textColor=HexColor("#555555"), spaceAfter=6*mm)
S["ci"] = ST("ci", fontSize=12, leading=18, alignment=TA_CENTER)
S["h1"] = ST("h1", fontSize=18, leading=28, spaceBefore=12*mm,
              spaceAfter=6*mm, textColor=HexColor("#1a3c6e"))
S["h2"] = ST("h2", fontSize=14, leading=22, spaceBefore=8*mm,
              spaceAfter=4*mm, textColor=HexColor("#2d5a8e"))
S["bd"] = ST("bd", fontSize=11, leading=20, alignment=TA_JUSTIFY,
              spaceBefore=2*mm, spaceAfter=3*mm, firstLineIndent=22)
S["dc"] = ST("dc", fontSize=10.5, leading=18, spaceBefore=4*mm,
              spaceAfter=4*mm, leftIndent=10*mm, rightIndent=10*mm,
              backColor=HexColor("#f5f5f0"), borderPadding=(8, 8, 8, 8))
S["rf"] = ST("rf", fontSize=9.5, leading=16, spaceBefore=1*mm,
              spaceAfter=1*mm, leftIndent=8*mm, firstLineIndent=-8*mm)


def P(style, text):
    return Paragraph(text, S[style])


def make_cover():
    r = [Spacer(1, 35*mm)]
    r.append(P("ct", "《人工智能应用》课程结课报告"))
    r.append(Spacer(1, 6*mm))
    r.append(P("cs", "AI浪潮中的计算机学生：从课堂到未来的思考"))
    r.append(Spacer(1, 25*mm))
    data = [
        ["专业：", "计算机科学与技术"],
        ["课程：", "人工智能应用"],
        ["报告题目：", "计算机专业学子对AI的学习与思考"],
        ["提交日期：", "2026年8月20日"],
    ]
    t = Table(data, colWidths=[32*mm, 90*mm])
    t.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, -1), "F"),
        ("FONTSIZE", (0, 0), (-1, -1), 12),
        ("ALIGN", (0, 0), (-1, -1), "LEFT"),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("TOPPADDING", (0, 0), (-1, -1), 5*mm),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 5*mm),
        ("GRID", (0, 0), (-1, -1), 0.5, grey),
    ]))
    r.append(t)
    r.append(PageBreak())
    return r


def make_body():
    r = []
    # All content as (style_name, text) tuples
    content = [
        ("h1", "前言"),
        ("bd", (
            "近几年来，人工智能正在以前所未有的速度重塑世界。还记得高中的时候才第一次听说什么豆"
            "包ai、什么AlphaGo，没想到几年后AI就已经渗透到生活的方方面面。到了大学，我的感觉是没"
            "有ai的日子将寸步难行。从各种大语言模型的不断涌现，到生成式AI的创意爆发，AI这一领域"
            "的技术迭代之快、影响范围之广，简直可以说是新一次科技革命了。"
            "作为计算机科学与技术专业的一名学生，本学期的《人工智能应用》课程为我打开了一扇观察与"
            "理解AI世界的窗口。本报告会从课程讲座学习体会、AI与计算机专业的交叉认识、个人感兴趣的"
            "AI方向，以及对学校课程建设的建议四个维度，系统梳理我的学习收获与思考。"
        )),
        ("h1", "一、课程讲座学习体会"),
        ("h2", "1.1 总览与印象最深的讲座"),
        ("bd", (
            "在本学期《人工智能应用》课程中，我印象里最深刻的是4月初的那场线下讲座，以及主讲教师介"
            "绍的当时挺火的一个AI模型——龙虾。"
        )),
        ("h2", "1.2 AI与大学生：认知的刷新"),
        ("bd", (
            "这场讲座从AI如何改变大学生的学习与生活这一贴近实际的视角切入。通过介绍我了解到越来越多"
            "的大学生已经开始使用各种各样的AI工具，但大部分人停留在聊天对话层面，只有少数同学使用过"
            "ai编程等高级操作。这一数据让我意识到——我们作为计算机专业的学生，应该积极学习和应用AI"
            "技术，去认真思考其背后的运行机制与潜在局限，甚至开发出更优秀的ai工具。"
        )),
        ("bd", (
            "讲座还讲到了AI对大学生能力要求的变化。以前我们总觉得学习嘛，就是多背多练、多刷题，"
            "但在AI时代，这些东西的价值好像越来越低了，反而是会问问题、能跨学科思考、懂得跟AI协作"
            "这些能力变得越来越重要。听完我挺有感触的——以前我遇到问题第一反应就是上网搜答案，"
            "现在才慢慢明白，能提出一个好问题，可能比找到答案本身更重要。"
        )),
        ("h2", "1.3 龙虾模型带来的震撼"),
        ("bd", (
            "当时老师介绍龙虾模型的时候，说实话，当时的我还并不清楚ai模型具体是什么东西，但有一个"
            "点让我印象特别深——它并不是那种什么都能干的超级大模型，而是在某些特定任务上做得特别"
            "好的小而精的模型。这让我第一次意识到，原来AI并不是越大越强，找准一个方向深耕下去，照"
            "样能做出很厉害的东西。想想我们以后的学习，好像也是这个道理。"
        )),
        ("bd", (
            "回头想想，这场讲座让我印象最深的其实不是技术本身，而是我第一次认真思考我们计算机人和AI"
            "到底是什么关系。以前我只把AI当工具，问个问题啥的完事，但听完之后我觉得，作为计算机专业"
            "的学生，我们不能只满足于当AI的使用者，更要去理解它、甚至参与创造它。AI时代最怕的"
            "不是被AI取代，而是我们根本不懂AI，只能被动地被它推着走。"
        )),
        ("h1", "二、人工智能与本人专业的交叉认识"),
        ("h2", "2.1 AI如何改变计算机科学与技术专业"),
        ("bd", (
            "我的专业是计算机科学与技术。说实话，这个专业跟AI的关系一直挺密切的——现在那些大模型、"
            "深度学习框架，底层用的还是我们学的数据结构、算法这些东西。不过这两年我明显感觉到，"
            "AI的发展反过来也在改变计算机专业本身，很多以前学的东西，现在的玩法已经完全不一样了。"
        )),
        ("bd", (
            "比如说搞研究这块，现在AI已经成了很多领域离不开的帮手，帮科学家预测蛋白质结构、"
            "帮工程师自动设计芯片，以前要好几年才能做完的事，现在快多了。再说写代码，以前我们总"
            "觉得程序员就是\u201c写代码的\u201d，但像GitHub Copilot这些工具出来以后，我发现写代码"
            "这件事本身正在被AI代劳。我自己在课程项目里也试过让AI帮我写代码、找bug，确实快很多。"
            "所以我觉得以后当程序员，重要的可能不是自己一行行敲代码，而是会提需求、能看懂AI写的"
            "代码、能把各个模块拼起来——说白了，就是从\u201c写代码的人\u201d变成\u201c指挥代码的人\u201d。"
        )),
        ("h2", "2.2 计算机专业学生应补充的能力"),
        ("bd", (
            "面对AI时代，我觉得作为计算机专业的学生，有些能力是必须补上的。首先是得真正搞懂机器"
            "学习、深度学习的基本原理，不然用AI永远只是\u201c知其然不知其所以然\u201d；其次是学会"
            "跟AI打交道，比如怎么把提示词写清楚，怎么用好各种AI工具；再就是得有工程思维，知道一个"
            "模型怎么从论文变成能跑的系统，还得考虑它靠不靠谱、安不安全；最后我觉得跨学科也很重要，"
            "AI现在哪儿都用得上，光懂技术不懂行业，很多问题其实做不好。"
        )),
        ("h1", "三、个人感兴趣的AI技术、应用与学习经历"),
        ("h2", "3.1 大语言模型与生成式AI"),
        ("bd", (
            "要说我最感兴趣的AI方向，那肯定是这两年最火的大语言模型了。记得ChatGPT刚出来那会儿，"
            "朋友圈全在讨论，感觉一夜之间AI就火遍全世界了。不过用着用着我也开始好奇：这些模型到底"
            "是真\u201c懂\u201d了，还是只是在\u201c猜\u201d下一个字？后来看了些资料才慢慢明白，"
            "大模型更像是一个\u201c统计学上的通才\u201d——它见过太多数据，所以能把话说得很像那么"
            "回事，但真要它做因果推理、理解常识，还是会露馅。想明白这一点之后，我用AI的时候反而"
            "多了一分冷静，该信的才信，该自己查的还是会自己查。"
        )),
        ("h2", "3.2 AI Coding：代码生成工具的实践体验"),
        ("bd", (
            "作为计算机专业的学生，AI编程工具肯定是要用的。这学期我在写作业、做小项目的时候经常用"
            "GitHub Copilot，说实话效率确实高，写那些重复性强的样板代码，AI基本上几秒钟就搞定了。"
            "但坑也不少——有时候它写的代码看着没问题，一跑就报错；有时候它还会一本正经地\u201c编造\u201d"
            "一个根本不存在的API，把人坑得够呛。所以我现在对AI写代码的态度是：可以用，但不能全信，"
            "写完一定要自己review一遍，测试也要自己跑。以前我总觉得调试是件麻烦事，现在反而觉得，"
            "能看懂AI写的代码、能帮它\u201c擦屁股\u201d，才是真本事。"
        )),
        ("h2", "3.3 AI Agent与具身智能"),
        ("bd", (
            "最近我又迷上了AI Agent这个方向。以前我们用AI都是\u201c你问我答\u201d，但现在这些AI"
            "智能体已经能自己拆解任务、一步一步去执行了，感觉就像真的雇了个\u201c数字员工\u201d。"
            "我看过一些AutoGPT之类的项目，挺震撼的，虽然现在还不太成熟，但我觉得这可能是AI从"
            "\u201c只会聊天\u201d走向\u201c真能干活\u201d的关键一步。我打算趁课余时间自学一些相关"
            "知识，有机会的话自己动手搭一个简单的AI Agent玩玩，光看别人做总觉得不过瘾。"
        )),
        ("h2", "3.4 学习路径与未来计划"),
        ("bd", (
            "上完这门课，我也给自己列了一份AI学习计划。第一步肯定是把基础打牢，系统学一下机器学习"
            "和深度学习的原理，网上像斯坦福的CS229这类公开课口碑都很好，打算抽空看看；第二步是动手，"
            "光看书没用，得多写代码，参加参加Kaggle比赛、逛逛开源项目，在实践中踩坑才有长进；第三步"
            "是保持对前沿的关注，比如AI Agent、多模态这些新方向，没事看看论文和资讯；最后就是希望"
            "能在毕业设计里真正把一个AI的东西做出来，把学到的知识用起来。"
        )),
        ("h1", "四、对学校后续AI类课程建设的建议与期待"),
        ("h2", "4.1 加强AI基础理论课程"),
        ("bd", (
            "这门课以讲座的形式展开，覆盖面很广，让我对AI有了整体的认识，这点我很喜欢。不过作为"
            "计算机专业的学生，我还是希望学校以后能开一些更系统的AI理论课，比如机器学习、深度学习、"
            "自然语言处理这些，光靠几场讲座总觉得有点不过瘾，想把底子打得更扎实一些。"
        )),
        ("h2", "4.2 增加实践与实训环节"),
        ("bd", (
            "另外就是希望多一些动手的机会。AI这东西光听不练真的学不会，希望学校能开一些实践类的"
            "课程或者工作坊，比如怎么用深度学习框架、怎么把训练好的模型部署上线。最好是项目驱动的，"
            "让我们\u201c做中学\u201d，把一个完整的东西从头到尾做一遍。要是学校再组织一些AI黑客松"
            "之类的比赛就更好了，大家一起做项目、互相切磋，肯定比一个人闷头学有意思多了。"
        )),
        ("h2", "4.3 开设AI伦理与安全专题"),
        ("bd", (
            "还有一个我觉得挺重要的，就是AI的伦理和安全问题。AI越来越强，带来的问题也越来越多，"
            "什么数据隐私泄露、算法有偏见、AI生成假视频骗人……这些离我们其实并不远。希望学校能开"
            "一些AI伦理相关的课或者讲座，让我们这些学技术的人，从一开始就有点\u201c科技责任感\u201d，"
            "别到时候只想着怎么把技术做出来，不想想它会带来什么后果。"
        )),
        ("h2", "4.4 推动AI与各专业交叉融合"),
        ("bd", (
            "最后就是希望学校能多搞一些AI和各个专业的交叉课。AI现在已经不只是计算机专业的事了，"
            "医学、金融、艺术、外语……每个行业都在用AI。要是学校能开一些AI+X的交叉课程，或者组织"
            "不同专业的同学一起做项目，既能让我们计算机的学生了解一下别的领域到底需要什么，也能让"
            "其他专业的同学学会用AI，这样对大家都好。"
        )),
        ("h1", "五、参考文献与结课报告撰写声明"),
        ("h2", "5.1 参考文献"),
        ("rf", "[1] 《人工智能应用》课程讲座资料，2026年。"),
        ("rf", "[2] 周志华.《机器学习》. 清华大学出版社，2016年。"),
        ("rf", "[3] Vaswani, A., et al. Attention Is All You Need. NeurIPS 2017."),
        ("rf", "[4] Brown, T. B., et al. Language Models are Few-Shot Learners. NeurIPS 2020."),
        ("rf", "[5] OpenAI. GPT-4 Technical Report. 2023."),
        ("rf", "[6] 李沐.《动手学深度学习》. 人民邮电出版社，2023年。"),
        ("rf", "[7] Russell, S., Norvig, P. Artificial Intelligence: A Modern Approach. 4th Ed., 2021."),
        ("h2", "5.2 结课报告撰写声明"),
        ("dc", (
            "本人承诺，本结课报告为本人基于课程学习、资料阅读和个人思考完成。报告撰写过程中，"
            "使用了人工智能工具（Deepseek）进行资料检索、提纲梳理以及局部语言润色辅助。对于AI工具"
            "辅助生成或修改的内容，本人已逐字逐句进行核查、筛选和修改，并对报告最终内容负全部责任。"
        )),
    ]

    # Add author info after declaration
    r.extend([
        Spacer(1, 8*mm),
        P("ci", "报告人：史前（计算机科学与技术专业）"),
        P("ci", "2026年8月20日"),
    ])
    for style, text in content:
        r.append(P(style, text))
    return r


def main():
    out = "人工智能应用课程结课报告.pdf"
    doc = SimpleDocTemplate(out, pagesize=A4, topMargin=25*mm,
                             bottomMargin=20*mm, leftMargin=25*mm,
                             rightMargin=25*mm)
    story = []
    story.extend(make_cover())
    story.extend(make_body())
    doc.build(story)
    print("PDF生成成功：" + out)


if __name__ == "__main__":
    main()