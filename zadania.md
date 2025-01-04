 
---

### 1. **Analiza dokumentu PDF i wyodrębnienie kluczowych danych**
   **Opis zadania:**  
   - Stwórz program w języku C#, który korzysta z Azure Document Intelligence do analizy dokumentu PDF (np. faktury).  
   - Wyodrębnij z dokumentu takie informacje jak: numer faktury, data, kwota brutto/netto, oraz nazwisko odbiorcy.  
   - Policz sumę wartości pozycji i zweryfikuj czy zgadza się ona z polem "suma".

---

### 2. **Transkrypcja mowy z pliku audio**
   **Opis zadania:**  
   - Użyj Azure Speech Services, aby przetworzyć plik audio (.mp3) na tekst.  
   - Wynik transkrypcji zapisz w pliku `.txt`.  
   - Program powinien pozwalać na wybór języka mowy (np. polski lub angielski).

---

### 3. **Transkrypcja filmu z YouTube z tłumaczeniem**
   **Opis zadania:**  
   - Stwórz aplikację, która:
     1. Pobierze napisy do filmu z YouTube 
     2. Przetłumaczy transkrypcję na wybrany język (np. angielski na polski) przy pomocy OpenAI ChatGPT.  
   - Wynikowy tekst zapisz w formacie `.docx`.
   - Program powinien mięc mozliwość wygenerowania streszczenia 

---


### 4. **Tworzenie streszczenia treści dokumentu PDF**
   **Opis zadania:**  
   - Korzystając z OpenAI API (np. GPT-4), załaduj plik PDF, a następnie prześlij jego zawartość do modelu, aby wygenerował streszczenie.  
   - Wygenerowane streszczenie zapisz w pliku i wyświetl w konsoli.  
   - Program powinien mieć możliwość wygenerowania streszczeń wielu plików umieszczonych w folderze 

---

### 5. **Automatyczne usuwanie tła z obrazu**
   **Opis zadania:**  
   - Wykorzystaj Azure Computer Vision do usunięcia tła z obrazu.  
   - Program powinien akceptować obraz w formacie `.jpg` lub `.png` i zapisać wynikowy plik z usuniętym tłem.  
   - Zaprezentuj porównanie oryginalnego obrazu i przetworzonego.

---

### 6. **Rozpoznawanie tekstu na obrazie (OCR)**
   **Opis zadania:**  
   - Stwórz program, który używa Azure OCR do analizy zdjęcia (np. zdjęcia dokumentu tożsamości).  
   - Wyodrębnij dane, takie jak imię, nazwisko, numer dokumentu.  
   - Posegreguj automatycznie dokumenty względem: płci lub/i innych kryteriów

---

### 7. **Generowanie obrazów na podstawie opisu**
   **Opis zadania:**  
   - Wykorzystując API OpenAI i model DALL-E, stwórz aplikację generującą obraz na podstawie dostarczonego przez użytkownika opisu.  

---

### 8. **Automatyczne tworzenie streszczenia artykułu internetowego**
   **Opis zadania:**  
   - Napisz program  który:
     1. Pobierze treść artykułu ze wskazanego URL.  
     2. Wyśle treść do OpenAI GPT w celu wygenerowania streszczenia.  
   - zrobi z tego streszczenie artkułu w PDF

---

### 9. **Asystent głosowy przekształcający mowę na tekst z odpowiedzią głosową**
   **Opis zadania:**  
   - Stwórz aplikację w C#, która:
     1. Konwertuje mowę użytkownika na tekst (Azure Speech Services).  
     2. Przekazuje tekst do modelu GPT w celu wygenerowania odpowiedzi.  
     3. Generuje odpowiedź głosową za pomocą Azure Text-to-Speech.  

---

### 10. **Tłumaczenie tekstu za pomocą OpenAI API**
   **Opis zadania:**  
   - Program w Pythonie powinien akceptować tekst w dowolnym języku i tłumaczyć go na inny język wybrany przez użytkownika (np. polski -> angielski).  


---

### 11. **Klasyfikacja obrazów na podstawie niestandardowego modelu Custom Vision**
   **Opis zadania:**  
   - Wykorzystaj Azure Custom Vision, aby stworzyć model klasyfikujący obrazy na dwie kategorie (np. „zdrowe owoce” i „zepsute owoce”).  
   - Napisz aplikację, która na podstawie zdjęcia zidentyfikuje, do której kategorii należy dany obraz.

---

### 12. **Wykrywanie popularnych obiektów na obrazie**
   **Opis zadania:**  
   - Stwórz aplikację, która wykorzystuje Azure Computer Vision do wykrywania obiektów na obrazie.  
   - Wyświetl listę rozpoznanych obiektów oraz ich współrzędne.  

---

### 13. **Tworzenie interfejsu chatbotowego w C#**
   **Opis zadania:**  
   - Napisz aplikację w C#, która działa jako chatbot, wykorzystując API OpenAI GPT.  
   - Program powinien pozwalać na prowadzenie wieloetapowej rozmowy z użytkownikiem w konsoli.

---

### 14. **Automatyczna analiza rozmów telefonicznych**
   **Opis zadania:**  
   - Używając Azure Speech Services, stwórz aplikację przetwarzającą nagrania rozmów telefonicznych w formacie `.mp3`.  
   - Wyodrębnij kluczowe informacje (np. numer kontaktowy, temat rozmowy) i zapisz wyniki w pliku JSON.

