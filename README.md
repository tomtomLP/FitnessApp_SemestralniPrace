# FitnessApp_SemestralniPrace
- Varianta: (a)
- Téma: Fitness aplikace pro evidenci tréninků

- Sledované entity: cviky, tréninkové plány, záznamy o tréninku, uživatel
- Vztahy: každý tréninkový plán obsahuje seznam cviků, každý záznam o tréninku je svázán s jedním plánem a obsahuje odcvičené váhy a opakování
- aplikace umožňuje uživateli vytvářet, editovat a mazat veškeré uvedené typy entit (cviky, plány)
- aplikace umožňuje zaznamenat probíhající trénink, zobrazit historii tréninků a vyhledávat cviky
- pro perzistenci dat je využito knihovny LiteDB (link: https://github.com/mbdavid/LiteDB)
- aplikace je postavena na platformě .NET s využitím frameworku Avalonia UI (vývoj pro macOS) v jazyce C#
