# -*- coding: utf-8 -*-
import re

data = open(r'C:\Users\shiqian\Peak\bulid\peak_new.pck', 'rb').read()
indep = len(re.findall(rb'"ARCHITECT\.talk\.SCOUT', data))
the = len(re.findall(rb'"THE_ARCHITECT\.talk\.SCOUT', data))
print('independent "ARCHITECT.talk.SCOUT:', indep)
print('"THE_ARCHITECT.talk.SCOUT:', the)
