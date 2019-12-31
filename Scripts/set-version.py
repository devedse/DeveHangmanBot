import re
import os

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

def setVersion(filePath):
  with open (filePath, 'r' ) as f:
    content = f.read()
    content_new = re.sub('(?<=<Version>).*(?=<\/Version>)', '1.0.0.1', content, flags = re.M)
    print(content_new)
  with open (filePath, 'w') as f:
    f.write(content_new)


setVersion('..\DeveHangmanBot\DeveHangmanBot.csproj')
setVersion('..\DeveHangmanBot.TelegramBot\DeveHangmanBot.TelegramBot.csproj')
setVersion('..\DeveHangmanBot.WebApp\DeveHangmanBot.WebApp.csproj')