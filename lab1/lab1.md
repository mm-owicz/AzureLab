# Ćwiczenie: Baza danych MS SQL Server w Azure

## Krok 1 Utworzenie konta w Azure

Zalogowano się w Azure Portal, przedłużając (po studiach inżynierskich) subskrypcję Azure for Students.

## Krok 2 Utworzenie instancji Azure SQL Database

Stworzono zasób SQL Database.

a. Tworzenie zasobu\
Wybrano opcję "Create" SQL Database

b. Konfiguracja projektu\
Stworzono nową grupę zasobów i wpisano nazwę dla bazy danych.

![Alt text](lab1_img_src/dbconfig1.png)

d. Konfiguracja serwera\
Stworzono nowy serwer.

![Alt text](lab1_img_src/lab1krok1server1.png)

![Alt text](lab1_img_src/lab1krok1server2.png)

e. Wybór opcji cenowych i rozmiaru \
Wybrano zasugerowane opcje rozmiarowe.
- General Purpose
- Serverless

![Alt text](lab1_img_src/dbconfig1.png)
![Alt text](lab1_img_src/dbconfigcena.png)

f. Dodatkowe ustawienia \
Wybrano domyślne ustawienia.

![Alt text](lab1_img_src/dbconfig3.png)


## Krok 3 Zatwierdzenie i wdrożenie
Zatwierdzono i utworzono zasób.

![Alt text](lab1_img_src/dbconfigdone.png)

## Krok 4 Połączenie z bazą danych

Połączono z bazą danych za pomocą SSMS.

## Krok 5: Tworzenie aplikacji
Stworzono aplikację .NET (w folderze Lab1App).

Podczas tworzenia bazy danych, nie było opcji wybrania przykładowej bazy, dlatego w ramach kroku 5, utworzono przykładową tabelę za pomocą SSMS. Wstawiono przykładowe dane:

![Alt text](lab1_img_src/ssmsdata1.png)

Po uruchomieniu aplikacji, wyświetlają się dane w konsoli użytkownika:


![Alt text](lab1_img_src/danekonsola1.png)

## Krok 6: Konfiguracja maszyny wirtualnej

Stworzono maszynę wirtualną z SQL Server.

![Alt text](lab1_img_src/vmconfig1.png)

![Alt text](lab1_img_src/vmconfig2.png)

![Alt text](lab1_img_src/vmconfig3.png)

Skonfigurowano reguły sieciowe:

![Alt text](lab1_img_src/vmconfigsecurity.png)

Uruchomiono maszynę wirtualną:

![Alt text](lab1_img_src/vmcomplete.png)

Połączono się przez SSMS:

![Alt text](lab1_img_src/vmconnection.png)


## Krok 7


## Konfiguracja Firewalla Azure SQL Database

Dodano mój obecny adres IP poprzez opcję "Set server firewall"

![Alt text](lab1_img_src/firewall.png)
