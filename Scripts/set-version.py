import re
import os

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

files = [f for f in os.listdir('.') if os.path.isfile(f)]
for f in files:
  print(f)

version = os.getenv('TRAVIS_BUILD_ID', 0)

print(f'Setting version: {version}')

def setVersion(filePath):
  with open (filePath, 'r' ) as f:
    content = f.read()
    content_new = re.sub('(?<=<Version>).*(?=<\/Version>)', f'1.0.0.{version}', content, flags = re.M)
    print(content_new)
  with open (filePath, 'w') as f:
    f.write(content_new)


setVersion('..\DeveHangmanBot\DeveHangmanBot.csproj')
setVersion('..\DeveHangmanBot.TelegramBot\DeveHangmanBot.TelegramBot.csproj')
setVersion('..\DeveHangmanBot.WebApp\DeveHangmanBot.WebApp.csproj')