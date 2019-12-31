import re
import os

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
parentdir = os.path.abspath(os.path.join(dname, os.pardir))
#os.chdir(parentdir)



buildId = os.getenv('TRAVIS_BUILD_NUMBER', 0)
version = "1.0.0.{}".format(buildId)

print("Setting version: {}".format(version))

def listFiles(path):
  print("Listing files in: {}".format(path))
  files = [f for f in os.listdir(path)]
  for f in files:
    print(f)

def setVersion(fileName):
  filePath = os.path.join(parentdir, fileName)
  with open (filePath, 'r' ) as f:
    content = f.read()
    content_new = re.sub('(?<=<Version>).*(?=<\/Version>)', version, content, flags = re.M)
    print(content_new)
  with open (filePath, 'w') as f:
    f.write(content_new)

listFiles('/home/travis/build/devedse/DeveHangmanBot/DeveHangmanBot')
listFiles('/home/travis/build/devedse/DeveHangmanBot')
listFiles('/home/travis/build/devedse')

setVersion('DeveHangmanBot/DeveHangmanBot.csproj')
setVersion('DeveHangmanBot.TelegramBot/DeveHangmanBot.TelegramBot.csproj')
setVersion('DeveHangmanBot.WebApp/DeveHangmanBot.WebApp.csproj')