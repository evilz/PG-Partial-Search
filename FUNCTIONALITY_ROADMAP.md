# Roadmap de Fonctionnalités - PG-Partial-Search

## 🎯 Vue d'ensemble
Améliorations structurées pour transformer le projet en une application complète de recherche type Elastic, tout en conservant la simplicité du modèle actuel.

---

## 🔥 Priorité Haute (Core Features)

### P1-01 - Recherche multi-champs indexée
**Description**: Ajouter un champ `FullName` concaténé avec un GIN index trigram séparé pour faciliter les recherches sur tout le profil
**Impact**: Permet de rechercher "john smith" ou "smith john" en un seul passage SQL  
**Étapes**:
- [ ] Ajouter colonne `FullName` dérivée dans la migration/seed
- [ ] Créer GIN index trigram sur une expression composite `(FirstName || ' ' || LastName)`
- [ ] Modifier le endpoint `/api/search` pour accepter `field` paramètre (names, full)
- [ ] Tester performance

**Fichier implémentation**: `Data/SearchIndexService.cs` ou extension dans l'API endpoint actuel  

---

### P1-02 - Filtres de recherche avancés
**Description**: Filtres côté client et serveur pour City et catégories d'utilisateurs  
**Impact**: Affine les résultats pour des utilisations réelles (recherche géographique, professionnelle)  
**Étapes**:
- [ ] Ajouter un cache/in-memory list des `uniqueCities` depuis le seed
- [ ] Ajouter paramètres de query: `city`, `minResults=N`
- [ ] UI ajout : dropdown City et badge de contrôle "Min Results"
- [ ] Tester combinaison de filtres + pagination

**Fichiers modifiés**: 
- `ApiService/SearchController.cs` - logique de filtre
- `PGPartialSearch.AppHost/appsettings.*.json` - ajouter la clé `cities` cache  

---

### P1-03 - Requête par email / téléphone (full-text)
**Description**: Recherche exacte ou partielle sur Email et Phone via GIN expression  
**Impact**: Permet de retrouver un utilisateur par son courriel ou numéro avec trigram aussi  
**Étapes**:
- [ ] Créer 2 index GIN express sur `Email` et `Phone` dans la startup migration (`IF NOT EXISTS`)
- [ ] Ajouter logique EF Core pour `.Where(p => (p.Email || ' ') LIKE '%query%')` via expression params trigram
- [ ] Endpoint API ajoute: `emailSearch`, `phoneSearch` booléens ou `searchOn=first|last|email|phone|all`

**Note**: pg_trgm sur VARCHAR fixe ~50/200 est efficace sans sur-indexer  

---

### P1-04 - Sortie / Tri multi-critères
**Description**: Contrôle de tri pour firstName, lastName, city avec `ASC`/`DESC`, combiné  
**Impact**: L'utilisateur peut ordonner ses résultats par critères pertinents 
**Étapes**:
- [ ] Ajouter paramètre query: `sortBy=name|city` et `sortOrder=asc|desc`
- [ ] Implémenter tri combiné côté SQL avec `ORDER BY firstName COLLATE "C" ASC, lastName ASC, city ASC`
- [ ] UI : sélection en liste déroulante près du champ de recherche

**Fichiers modifiés**: `ApiService/SearchController.cs`, ajout paramètres de tri dans l'option  

---

## 🌟 Priorité Moyenne (UX Enhancement)

### P2-01 - Mise en surbrillance des correspondances
**Description**: Colorer dans les résultats les parties du nom qui correspondent à la recherche  
**Impact**: Améliore considérablement la lisibilité et l'expérience utilisateur  
**Étapes**:
- [ ] Fonction utilitaire regex `SearchHighlighted(string text, string query)` en JS côté client :
  - Escape le query: `\b\(query\)` ou simple highlight de sous-chaine
  - Envoyer via `/api/search` pour récupérer les résultats bruts (déjà fait)
  - Parser la réponse JSON, re-insérer du `<mark>` HTML autour des parties trouvées
- [ ] Style CSS déjà prévu dans le projet (`highlight {background #ffeb3b}`)  

**Fichiers modifiés**: 
- `ApiService/wwwroot/search.html` - ajouter fonction `highlightSearchResults()` 

---

### P2-02 - Indicateur de performance / temps de réponse
**Description**: Afficher le temps de requête dans le footer / résultats  
**Impact**: Permet de voir concrètement la puissance des index GIN  
**Étapes**:
- [ ] Middleware d'horodatage sur les réponses / mesure `Stopwatch` autour du query execution
- [ ] Utiliser `QueryTimeoutInSecs=2.0` EF Core par défaut pour limiter le temps en cas d'erreur

---

## 🧪 Priorité Basse (Bonus)

### P3-01 - Interface avancée de recherche
**Description**: Mise en place d'une barre latérale ou panneau avec historique récent des recherches  
**Impact**: Permet une interaction avancée sans réintroduire les champs  

---

### P3-02 - Dashboard d'analyse globale
**Description**: Requête par `COUNT(*)` des statistiques globales (total personnes, villes disponibles) 
**Étapes**:
- [ ] Ajouter endpoint `/api/stats` qui retourne object `{ total: 10000, citiesCount: N, avgFirstNameLength: X }`  
- [ ] UI ajout : tableau "Top Cities" dans la bar latérale ou modale

---

### P3-03 - Export des résultats (CSV / JSON compact)
**Description**: Télécharger les résultats sous forme de fichier CSV (tableau → export)  
**Impact**: Utilitaires pour power users  

---

## 📋 Suivi d'implémentation global

| #      | Fonctionnalité          | Priorité     | Statut    | Fichier(s) impliqué(s)                                              |
|--------|-------------------------|--------------|-----------|---------------------------------------------------------------------|
| P1-01  | Recherche multi-champs indexée | High        | ⬜ À faire   | `SearchIndexService.cs`, `ApiService/Program.cs` (migration)           |
| P1-02  | Filtres avancés de recherche                           | High        | ⬜ À faire   | `SearchController.cs`, `appsettings.*.json` (cache)                    |
| P1-03  | Requête par email/téléphone                            | High        | ⬜ À faire   | `Database/Migration.cs`, `ApiService/Program.cs`                      |
| P1-04  | Tri multi-critères / retour                             | High        | ⬜ À faire   | `SearchController.cs`, `api-search.cshtml.sample`                     |
| P2-01  | Mise en surbrillance des correspondances              | Medium      | ⬜ À faire   | `wwwroot/search.html`                                                 |
| P2-02  | Indicateur temps de requête                           | Medium      | ⬜ Deviser   | `Database/Middleware.cs`, logique d'horodatage                        |
| P3-01  | Panneau historique des recherches                     | Low         | ⬜ À faire   | `search.html`                                                         |
| P3-02  | Dashboard global d'analyse                            | Low         | ⬜ À faire   | `Database/StatsController.cs`, `/api/stats`                           |
| P3-03  | Export CSV des résultats                              | Low         | ⬜ À faire   | `SearchController.cs`, ajout utilitaire de génération CSV             |

**Notes**: Priorité basée sur l'importance pour "production" réelle, compatibilité Elastic et la complexité d'implémentation.  
Date mise à jour: 2026-07-18  
