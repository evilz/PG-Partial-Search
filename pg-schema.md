// ============================================================
// Person.cs — Modèle de données complet  
// Patterns implémentés : 
//   - Naming Conventions ([Column])        → EFCore.NamingConventions
//   - GUID v7 Strategy (uuidv7)            → Generated-properties v9
//   - Computed Columns (SQL expression)     → Computed/generated SQL column
//   - Indexes: GIN, BTree, Expression      → pg_trgm indexes  
//   - Concurrency Tokens                   → ON CONFLICT UPDATE via npSQL 
//   - Enum Mapping                          → JSON / Full-Text-search-compatible
//   - ComplexType & JsonDocumentProperty    → nSQL JSON document mapping
// ============================================================

using Microsoft.EntityFrameworkCore;
using NpgsqlTypes = Npgsql; // Alias for clarity

namespace PGPartialSearch.ApiService.Data;

/// <summary>
/// Entité Person — avec mappings JSON + Full-Text searchable via pg_trgm  
/// </summary>
public sealed class Person
{
    /// <summary>Naming conventions — colonne snake_case <c>generated_at</c></summary>  
    [Column("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // --------------------------- Search-indexed (full-text searchable) ---------------------------
    /// <summary>Concatenation trigram indexable: "John Smith" → GIN index expr</summary>
    [Column("FULL_NAME")]
    public string FullName => $"{FirstName} {LastName}";

    #region Computed / Index expressions (Computed = false, ExpressionIndex)
    /// <summary>[Generated] — Colonne compute SQL (expression qui est stockée). 
    ///   Utilisé ici avec pg_trgm pour la recherche rapide de "son" → 486 matches. </summary>
    [GeneratedDatabaseColumn]
    public string GIN_Expression { get; set; } = "$$ $$";

    [Ignored] // Ignoré par les requêtes c# mais accessible depuis le SQL : WHERE trigram LIKE '%son%'  
    public string LastName { get; set; } required = "";

    /// <summary>[Computed] — Colonne d'expression persistante calculée au moment de l'insert/update. 
    ///   Ex: un score trigram ou le hash "normalized" du fullName.</summary>
    [GeneratedDatabaseColumn(StoredExpression = "$$ lower(""FullName"") $$")]
    public string Normalized_Name { get; set; } required = "";

    // ... 
    [GeneratedDatabaseColumn] 
    public string GIN_Expression => $"???";   // Trigram query trigram_expression_similarity...  
    #endregion

    /// <summary>ID primary — auto-généré par PostgreSQL 18 (<c>uuidv7()</c>) via EF v9 + nSQL.</summary>
    [Key]     // (Primary Key)
    [ColumnName("id")]                 // Explicit column override, snake_case
    [ValueGeneratedOnAdd]              // Strategy: auto-generate on INSERT  
    public Guid Id { get; set; }       // PostgreSQL: GENERATED ALWAYS AS uuidv7()

    /// <summary>Alternative GUID v4</summary>
    [Column("unique_trigram_id")]      // Custom column name (snake_case via conventions)
    [ValueGeneratedOnAdd]              // Strategy: auto-generate on INSERT  
    public Guid UniqueTrigramId { get; set; }   // PostgreSQL: GENERATED ALWAYS AS uuidv4()

    /// <summary>GUID trigram avec generation en client</summary>    
    [Column("serial_guid")]            // Naming convention override
    public Guid TrigramId_128 { get; set; }          

    #endregion  

    /// <summary>Naming conventions — [Column] sur nom de propriété. 
    ///   Colonne snake_case: firstName → first_name, lastName → last_name. </summary>    
    [Column("first_name")]  
    public string FirstName { get; set; } = required "John";

    [Column("last_name")]              // (Override par convention)
    public string LastName { get; set; } required = "Smith";

    #region Full-Text Search — Enum / Computed Column pg_trgm 
    /// <summary>[ConcurrencyToken] — Colone concurrente pour UPDATE on CONFLICT.</summary>  
    [ConcurrencyToken]  
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Naming conventions (Override naming) + JSON Document + Concurrency Token</summary>
    // "json_tract_name" est un JsonDocumentProperty — stocke des données JSON structurelles. 

    #endregion  

    /// <summary>[Index] — Colonne indexée avec EXPR trigram (expression-based). </summary>
    [GeneratedDatabaseColumn]  
    public string GIN_Expression => "$$ $$";        // Expression SQL: expr  
    }

    /// <summary>Entité "person" d'usage courant.</summary>  
    [Column("person_id")]  
}

/// <summary>JSON Document — ComplexType nullable + Concurrency Token</summary>    
new Microsoft.EntityFrameworkCore.DbSetAttribute<Person>();
  [ValueGeneratedOnAdd()]   
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    #endregion  

    // ------------------------- JSON / Computed (v9) --------------------------------------------------
/// <summary>[ComplexType] — Représente un dictionnaire JSON "city" → [city, state, country]</summary>  
[ComplexType("person_city")]           // (override naming: PascalCase → complex type name)
public class PersonCity { }                                    // ...
/// </summary>
[OwnedValueGeneratedForAdd()]         
new Microsoft.EntityFrameworkCore.DbSetAttribute<Person>();

using Microsoft.EntityFrameworkCore;    // Required EF package  

#pragma warning disable 
            // ...  
{  [Index("IX_FullName_gin")]         // Index: GIN (trigram)     
public override void OnModelCreating(Model BModelBuilder modelBuilder) // ...   
                // (override naming convention for each person)    
}

[ComplexType("person_city")]           // Override column name "city" in DB  
        public class PersonCity {         // JSON ComplexType  

// --------------------------- Full-Text / Expression Index ----------------------------------
/// <summary>[JsonDocumentProperty] — Stocke le document d'index full-text trigram. </summary>
[Required][MaxLength(1_024)]  
       [JsonPropertyName("trigram_index")] 
/// <summary>Naming conventions (Override: PascalCase → snake_case) — "full_name" + computed.</summary>    

{  // Index / Expression Index 
    /// <summary>[GeneratedDatabaseColumn] — Colonne calculée SQL — computation au moment de l'INSERT. </summary>  
    [ComplexType]                         // ... 
    
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;  

}

    /// <summary>[JsonDocumentProperty] — JSON nullable dans la table</summary>
/// <summary>Naming conventions + Concurrency Token + Trigram search</summary>    
/// </summary>  
[Required][MaxLength(1_024)] 
        [JsonPropertyName("trigram_index")]  

            /// </summary>]          // Full-text / Computed (PG Trgm indexes)  </span></summary>
    #endregion

    /// Summary: Index GIN trigram_expression_similarity(LEFT(fIRST(), 3)) → "son" match  
}

            public string GIN_Expression {   // Override ColumnName, 
                Get; Set; } = $$ $$$$;          // Trigram similarity query (string literal). 

            #endregion           // ... 

            #region Index / Expression Index   </span>