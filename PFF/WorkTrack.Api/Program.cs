using Microsoft.EntityFrameworkCore;
using WorkTrack.Infrastructure;
using WorkTrack.Domain.Entities;
using WorkTrack.Application;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WorkTrack.Api.Seeding; // <-- SeedData.SeedDevAsync
using QuestPDF.Infrastructure;
using Microsoft.OpenApi.Models;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Swagger / endpoints
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// Swagger (avec support Bearer)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WorkTrack API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Tape: Bearer {votre_token_JWT}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});
// PostgreSQL (clé appsettings: "DefaultConnection"; fallback env: WORKTRACK_CS)
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? Environment.GetEnvironmentVariable("WORKTRACK_CS");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Chaîne de connexion manquante. Ajoute ConnectionStrings:DefaultConnection ou WORKTRACK_CS.");
    opt.UseNpgsql(cs);
});

// CORS pour Blazor WASM en dev (à ajuster)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("wasm", p => p
        .WithOrigins(
            "http://localhost:5153", "http://localhost:7106", // tes anciens ports si tu les utilises
            "http://localhost:5023", "http://localhost:7106", "https://localhost:7106"  // <-- mets ICI le(s) port(s) réel(s) du client
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});


// ===== JWT Auth =====
var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("EnseignantOnly",  p => p.RequireRole(nameof(RoleUtilisateur.Enseignant)));
    opt.AddPolicy("ResponsableOnly", p => p.RequireRole(nameof(RoleUtilisateur.Responsable)));
    opt.AddPolicy("AdminOnly",       p => p.RequireRole(nameof(RoleUtilisateur.Administrateur)));
});
var app = builder.Build();

// Migrate + Seed (dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (app.Environment.IsDevelopment())
    {
        var legacy = await db.Utilisateurs
            .Where(u => string.IsNullOrEmpty(u.MotDePasseHash) || !u.MotDePasseHash.StartsWith("$2")) // "$2" = préfixe BCrypt
            .ToListAsync();

        if (legacy.Count > 0)
        {
            foreach (var u in legacy)
                u.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("dev"); // on force "dev" pour tests

            await db.SaveChangesAsync();
        }

        await SeedData.SeedDevAsync(db);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("wasm");

// =====================================================
//                   Minimal API
// =====================================================

// ---------- UTILISATEURS ----------
var utilisateurs = app.MapGroup("/api/utilisateurs").WithTags("Utilisateurs");
utilisateurs.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Utilisateurs.OrderBy(u => u.Nom).ToListAsync(ct)));
utilisateurs.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var u = await db.Utilisateurs.FindAsync(new object?[] { id }, ct);
    return u is null ? Results.NotFound() : Results.Ok(u);
});
utilisateurs.MapPost("", async (AppDbContext db, Utilisateur u, CancellationToken ct) =>
{
    if (u.Id == Guid.Empty) u.Id = Guid.NewGuid();
    db.Utilisateurs.Add(u); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/utilisateurs/{u.Id}", u);
});
utilisateurs.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Utilisateur input, CancellationToken ct) =>
{
    var u = await db.Utilisateurs.FindAsync(new object?[] { id }, ct);
    if (u is null) return Results.NotFound();
    u.Prenom = input.Prenom; u.Nom = input.Nom; u.Email = input.Email;
    u.Telephone = input.Telephone; u.MotDePasseHash = input.MotDePasseHash;
    u.Role = input.Role; u.Actif = input.Actif;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
utilisateurs.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var u = await db.Utilisateurs.FindAsync(new object?[] { id }, ct);
    if (u is null) return Results.NotFound();
    db.Utilisateurs.Remove(u); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- ENSEIGNANTS ----------
var enseignants = app.MapGroup("/api/enseignants").WithTags("Enseignants");
enseignants.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Enseignants.ToListAsync(ct)));
enseignants.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var e = await db.Enseignants.FindAsync(new object?[] { id }, ct);
    return e is null ? Results.NotFound() : Results.Ok(e);
});
enseignants.MapPost("", async (AppDbContext db, Enseignant e, CancellationToken ct) =>
{
    if (e.Id == Guid.Empty) e.Id = Guid.NewGuid();
    db.Enseignants.Add(e); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/enseignants/{e.Id}", e);
});
enseignants.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Enseignant input, CancellationToken ct) =>
{
    var e = await db.Enseignants.FindAsync(new object?[] { id }, ct);
    if (e is null) return Results.NotFound();
    e.UtilisateurId = input.UtilisateurId; e.Specialite = input.Specialite;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
enseignants.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var e = await db.Enseignants.FindAsync(new object?[] { id }, ct);
    if (e is null) return Results.NotFound();
    db.Enseignants.Remove(e); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- APPRENANTS ----------
var apprenants = app.MapGroup("/api/apprenants").WithTags("Apprenants");
apprenants.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Apprenants.ToListAsync(ct)));
apprenants.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var a = await db.Apprenants.FindAsync(new object?[] { id }, ct);
    return a is null ? Results.NotFound() : Results.Ok(a);
});
apprenants.MapPost("", async (AppDbContext db, Apprenant a, CancellationToken ct) =>
{
    if (a.Id == Guid.Empty) a.Id = Guid.NewGuid();
    db.Apprenants.Add(a); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/apprenants/{a.Id}", a);
});
apprenants.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Apprenant input, CancellationToken ct) =>
{
    var a = await db.Apprenants.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    a.UtilisateurId = input.UtilisateurId; a.PromotionId = input.PromotionId;
    a.Matricule = input.Matricule; a.DateNaissance = input.DateNaissance; a.Statut = input.Statut;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
apprenants.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var a = await db.Apprenants.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    db.Apprenants.Remove(a); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- FILIERES ----------
var filieres = app.MapGroup("/api/filieres").WithTags("Filières");
filieres.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Filieres.ToListAsync(ct)));
filieres.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var f = await db.Filieres.FindAsync(new object?[] { id }, ct);
    return f is null ? Results.NotFound() : Results.Ok(f);
});
filieres.MapPost("", async (AppDbContext db, Filiere f, CancellationToken ct) =>
{
    if (f.Id == Guid.Empty) f.Id = Guid.NewGuid();
    db.Filieres.Add(f); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/filieres/{f.Id}", f);
});
filieres.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Filiere input, CancellationToken ct) =>
{
    var f = await db.Filieres.FindAsync(new object?[] { id }, ct);
    if (f is null) return Results.NotFound();
    f.Nom = input.Nom; f.Niveau = input.Niveau; f.ResponsableId = input.ResponsableId;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
filieres.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var f = await db.Filieres.FindAsync(new object?[] { id }, ct);
    if (f is null) return Results.NotFound();
    db.Filieres.Remove(f); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- PROMOTIONS ----------
var promotions = app.MapGroup("/api/promotions").WithTags("Promotions");
promotions.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Promotions.ToListAsync(ct)));
promotions.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var p = await db.Promotions.FindAsync(new object?[] { id }, ct);
    return p is null ? Results.NotFound() : Results.Ok(p);
});
promotions.MapPost("", async (AppDbContext db, Promotion p, CancellationToken ct) =>
{
    if (p.Id == Guid.Empty) p.Id = Guid.NewGuid();
    db.Promotions.Add(p); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/promotions/{p.Id}", p);
});
promotions.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Promotion input, CancellationToken ct) =>
{
    var p = await db.Promotions.FindAsync(new object?[] { id }, ct);
    if (p is null) return Results.NotFound();
    p.FiliereId = input.FiliereId; p.Nom = input.Nom; p.AnneeScolaire = input.AnneeScolaire; p.EffectifPrevu = input.EffectifPrevu;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
promotions.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var p = await db.Promotions.FindAsync(new object?[] { id }, ct);
    if (p is null) return Results.NotFound();
    db.Promotions.Remove(p); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- MODULES ----------
var modules = app.MapGroup("/api/modules").WithTags("Modules");
modules.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Modules.ToListAsync(ct)));
modules.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var m = await db.Modules.FindAsync(new object?[] { id }, ct);
    return m is null ? Results.NotFound() : Results.Ok(m);
});
modules.MapPost("", async (AppDbContext db, Module m, CancellationToken ct) =>
{
    if (m.Id == Guid.Empty) m.Id = Guid.NewGuid();
    db.Modules.Add(m); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/modules/{m.Id}", m);
});
modules.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Module input, CancellationToken ct) =>
{
    var m = await db.Modules.FindAsync(new object?[] { id }, ct);
    if (m is null) return Results.NotFound();
    m.Code = input.Code; m.Nom = input.Nom; m.Coefficient = input.Coefficient; m.Ects = input.Ects;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
modules.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var m = await db.Modules.FindAsync(new object?[] { id }, ct);
    if (m is null) return Results.NotFound();
    db.Modules.Remove(m); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- ANNEES ----------
var annees = app.MapGroup("/api/annees").WithTags("Années académiques");
annees.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Annees.ToListAsync(ct)));
annees.MapPost("", async (AppDbContext db, AnneeAcademique a, CancellationToken ct) =>
{
    if (a.Id == Guid.Empty) a.Id = Guid.NewGuid();
    db.Annees.Add(a); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/annees/{a.Id}", a);
});
annees.MapPut("/{id:guid}", async (Guid id, AppDbContext db, AnneeAcademique input, CancellationToken ct) =>
{
    var a = await db.Annees.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    a.Libelle = input.Libelle; a.DateDebut = input.DateDebut; a.DateFin = input.DateFin;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
annees.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var a = await db.Annees.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    db.Annees.Remove(a); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- SEMESTRES ----------
var semestres = app.MapGroup("/api/semestres").WithTags("Semestres");
semestres.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Semestres.ToListAsync(ct)));
semestres.MapPost("", async (AppDbContext db, Semestre s, CancellationToken ct) =>
{
    if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
    db.Semestres.Add(s); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/semestres/{s.Id}", s);
});
semestres.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Semestre input, CancellationToken ct) =>
{
    var s = await db.Semestres.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    s.AnneeId = input.AnneeId; s.Libelle = input.Libelle; s.Rang = input.Rang;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
semestres.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Semestres.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    db.Semestres.Remove(s); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- AFFECTATIONS ----------
var affectations = app.MapGroup("/api/affectations").WithTags("Affectations");
affectations.MapGet("", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Affectations.ToListAsync(ct)));
affectations.MapPost("", async (AppDbContext db, AffectationModule a, CancellationToken ct) =>
{
    if (a.Id == Guid.Empty) a.Id = Guid.NewGuid();
    db.Affectations.Add(a); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/affectations/{a.Id}", a);
});
affectations.MapPut("/{id:guid}", async (Guid id, AppDbContext db, AffectationModule input, CancellationToken ct) =>
{
    var a = await db.Affectations.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    a.ModuleId = input.ModuleId; a.PromotionId = input.PromotionId; a.EnseignantId = input.EnseignantId;
    a.SemestreId = input.SemestreId; a.VolumeHorairePrevu = input.VolumeHorairePrevu;
    a.DateDebut = input.DateDebut; a.DateFin = input.DateFin;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
affectations.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var a = await db.Affectations.FindAsync(new object?[] { id }, ct);
    if (a is null) return Results.NotFound();
    db.Affectations.Remove(a); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// ---------- SEANCES (inclut today/roster/attendance + CRUD + lock/unlock + filtres) ----------
var seancesGroup = app.MapGroup("/api/seances").WithTags("Séances");

seancesGroup.MapGet("/today", async (AppDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.Now;
    var start = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
    var end = start.AddDays(1);

    var q = from s in db.Seances
            join a in db.Affectations on s.AffectationId equals a.Id
            join m in db.Modules on a.ModuleId equals m.Id
            join p in db.Promotions on a.PromotionId equals p.Id
            where s.Debut >= start && s.Debut < end
            orderby s.Debut
            select new SeanceDto(s.Id, s.Debut, s.Fin, s.Salle, m.Code, m.Nom, p.Nom);

    return Results.Ok(await q.ToListAsync(ct));
});

seancesGroup.MapGet("/{seanceId:guid}/roster", async (Guid seanceId, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { seanceId }, ct);
    if (s is null) return Results.NotFound();
    var aff = await db.Affectations.FindAsync(new object?[] { s.AffectationId }, ct);
    if (aff is null) return Results.NotFound();

    var roster = from a in db.Apprenants
                 join u in db.Utilisateurs on a.UtilisateurId equals u.Id
                 where a.PromotionId == aff.PromotionId
                 orderby u.Nom, u.Prenom
                 select new RosterStudentDto(a.Id, a.Matricule, u.Nom + " " + u.Prenom);
    return Results.Ok(await roster.ToListAsync(ct));
});

seancesGroup.MapPost("/{seanceId:guid}/attendance", async (
    Guid seanceId, AppDbContext db, List<AttendanceMarkDto> items, CancellationToken ct) =>
{
    var seance = await db.Seances.FirstOrDefaultAsync(x => x.Id == seanceId, ct);
    if (seance is null) return Results.NotFound();
    if (seance.EstVerrouillee) return Results.BadRequest("Séance verrouillée");

    foreach (var i in items)
    {
        var p = await db.Presences.FirstOrDefaultAsync(x => x.SeanceId == seanceId && x.ApprenantId == i.ApprenantId, ct);
        if (p is null)
        {
            p = new Presence
            {
                Id = Guid.NewGuid(),
                SeanceId = seanceId,
                ApprenantId = i.ApprenantId,
                MarquePar = Guid.Empty // TODO: remplacer par l'ID utilisateur (JWT)
            };
            db.Presences.Add(p);
        }
        p.Statut = i.Statut switch
        {
            AttendanceStatusDto.Present => StatutPresence.Present,
            AttendanceStatusDto.Absent  => StatutPresence.Absent,
            _                           => StatutPresence.Retard
        };
        p.MinutesRetard = (short?)i.MinutesRetard;
        p.Commentaire   = i.Commentaire;
        p.MarqueLe      = DateTimeOffset.UtcNow;
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok();
});

// LISTE COMPLETE AVEC STATUT
seancesGroup.MapGet("/{seanceId:guid}/attendance/list", async (Guid seanceId, AppDbContext db, CancellationToken ct) =>
{
    var seance = await db.Seances.FindAsync(new object?[] { seanceId }, ct);
    if (seance is null) return Results.NotFound();
    var aff = await db.Affectations.FindAsync(new object?[] { seance.AffectationId }, ct);
    if (aff is null) return Results.NotFound();

    var rows = await (
        from a in db.Apprenants
        join u in db.Utilisateurs on a.UtilisateurId equals u.Id
        where a.PromotionId == aff.PromotionId
        join pr0 in db.Presences.Where(p => p.SeanceId == seanceId)
            on a.Id equals pr0.ApprenantId into gj
        from pr in gj.DefaultIfEmpty()
        orderby u.Nom, u.Prenom
        select new AttendanceRowDto(
            a.Id,
            a.Matricule,
            u.Nom + " " + u.Prenom,
            pr == null ? "NonSaisi" :
                (pr.Statut == StatutPresence.Present ? "Present" :
                 pr.Statut == StatutPresence.Absent  ? "Absent"  : "Retard"),
            pr != null ? (int?)pr.MinutesRetard : null,
            pr != null ? pr.Commentaire : null
        )
    ).ToListAsync(ct);

    return Results.Ok(rows);
});

// PDF
seancesGroup.MapGet("/{seanceId:guid}/attendance/pdf", async (Guid seanceId, AppDbContext db, CancellationToken ct) =>
{
    var seance = await db.Seances.FindAsync(new object?[] { seanceId }, ct);
    if (seance is null) return Results.NotFound();
    var aff = await db.Affectations.FindAsync(new object?[] { seance.AffectationId }, ct);
    if (aff is null) return Results.NotFound();

    var meta = await (
        from a in db.Affectations
        join m in db.Modules on a.ModuleId equals m.Id
        join p in db.Promotions on a.PromotionId equals p.Id
        where a.Id == aff.Id
        select new { Module = m.Code + " - " + m.Nom, Promotion = p.Nom }
    ).FirstAsync(ct);

    var rows = await (
        from ap in db.Apprenants
        join u in db.Utilisateurs on ap.UtilisateurId equals u.Id
        where ap.PromotionId == aff.PromotionId
        join pr0 in db.Presences.Where(p => p.SeanceId == seanceId)
            on ap.Id equals pr0.ApprenantId into gj
        from pr in gj.DefaultIfEmpty()
        orderby u.Nom, u.Prenom
        select new AttendanceRowDto(
            ap.Id,
            ap.Matricule,
            u.Nom + " " + u.Prenom,
            pr == null ? "NonSaisi" :
                (pr.Statut == StatutPresence.Present ? "Present" :
                 pr.Statut == StatutPresence.Absent  ? "Absent"  : "Retard"),
            pr != null ? (int?)pr.MinutesRetard : null,
            pr != null ? pr.Commentaire : null
        )
    ).ToListAsync(ct);

    var bytes = WorkTrack.Api.Pdf.AttendancePdf.Create(
        titre: "Feuille de présence",
        sousTitre: $"{meta.Promotion} • {meta.Module} • {seance.Debut.LocalDateTime:dd/MM/yyyy HH:mm} - {seance.Fin.LocalDateTime:HH:mm} • Salle {seance.Salle}",
        rows: rows
    );

    var fileName = $"Presence_{meta.Promotion}_{seance.Debut:yyyyMMdd_HHmm}.pdf";
    return Results.File(bytes, "application/pdf", fileName);
});

// Liste + filtres
seancesGroup.MapGet("", async (
    AppDbContext db, DateTimeOffset? from, DateTimeOffset? to, Guid? enseignantId, Guid? promotionId, CancellationToken ct) =>
{
    var q = from s in db.Seances
            join a in db.Affectations on s.AffectationId equals a.Id
            join m in db.Modules on a.ModuleId equals m.Id
            join p in db.Promotions on a.PromotionId equals p.Id
            select new { s, a, m, p };

    if (from.HasValue) q = q.Where(x => x.s.Debut >= from.Value);
    if (to.HasValue) q = q.Where(x => x.s.Debut < to.Value);
    if (enseignantId.HasValue) q = q.Where(x => x.a.EnseignantId == enseignantId.Value);
    if (promotionId.HasValue) q = q.Where(x => x.a.PromotionId == promotionId.Value);

    var list = await q.OrderBy(x => x.s.Debut)
        .Select(x => new SeanceDto(x.s.Id, x.s.Debut, x.s.Fin, x.s.Salle, x.m.Code, x.m.Nom, x.p.Nom))
        .ToListAsync(ct);

    return Results.Ok(list);
});

seancesGroup.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { id }, ct);
    return s is null ? Results.NotFound() : Results.Ok(s);
});
seancesGroup.MapPost("", async (AppDbContext db, Seance s, CancellationToken ct) =>
{
    if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
    if (s.Fin <= s.Debut) return Results.BadRequest("Fin doit être > Début.");
    db.Seances.Add(s); await db.SaveChangesAsync(ct);
    return Results.Created($"/api/seances/{s.Id}", s);
});
seancesGroup.MapPut("/{id:guid}", async (Guid id, AppDbContext db, Seance input, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    if (input.Fin <= input.Debut) return Results.BadRequest("Fin doit être > Début.");
    s.AffectationId = input.AffectationId; s.Debut = input.Debut; s.Fin = input.Fin;
    s.Salle = input.Salle; s.EstVerrouillee = input.EstVerrouillee;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
seancesGroup.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    db.Seances.Remove(s); await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
seancesGroup.MapPost("/{id:guid}/lock", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    s.EstVerrouillee = true; await db.SaveChangesAsync(ct); return Results.NoContent();
});
seancesGroup.MapPost("/{id:guid}/unlock", async (Guid id, AppDbContext db, CancellationToken ct) =>
{
    var s = await db.Seances.FindAsync(new object?[] { id }, ct);
    if (s is null) return Results.NotFound();
    s.EstVerrouillee = false; await db.SaveChangesAsync(ct); return Results.NoContent();
});

// ---------- PRESENCES (listage/filtre simple) ----------
var presences = app.MapGroup("/api/presences").WithTags("Présences");
presences.MapGet("", async (
    AppDbContext db, Guid? seanceId, Guid? apprenantId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct) =>
{
    var q = from pr in db.Presences
            join s in db.Seances on pr.SeanceId equals s.Id
            select new { pr, s };

    if (seanceId.HasValue) q = q.Where(x => x.pr.SeanceId == seanceId.Value);
    if (apprenantId.HasValue) q = q.Where(x => x.pr.ApprenantId == apprenantId.Value);
    if (from.HasValue) q = q.Where(x => x.s.Debut >= from.Value);
    if (to.HasValue) q = q.Where(x => x.s.Debut < to.Value);

    var list = await q.OrderByDescending(x => x.s.Debut).Select(x => x.pr).ToListAsync(ct);
    return Results.Ok(list);
});

// ---------- RAPPORTS ----------
var reports = app.MapGroup("/api/reports").WithTags("Rapports");

// Taux d'assiduité par apprenant (sur période) pour une promotion

reports.MapGet("/promotion-attendance", async (
    AppDbContext db,
    Guid promotionId,
    DateTimeOffset @from,
    DateTimeOffset @to,
    CancellationToken ct) =>
{
    var q = from pr in db.Presences
            join s in db.Seances on pr.SeanceId equals s.Id
            join a in db.Affectations on s.AffectationId equals a.Id
            where a.PromotionId == promotionId
                  && s.Debut >= @from
                  && s.Debut  < @to
            group pr by pr.ApprenantId into g
            select new
            {
                ApprenantId = g.Key,
                Presents = g.Count(x => x.Statut == StatutPresence.Present || x.Justifie),
                Total    = g.Count(),
                Taux     = g.Count() == 0 ? 0.0 :
                           (g.Count(x => x.Statut == StatutPresence.Present || x.Justifie) * 100.0) / g.Count()
            };

    return Results.Ok(await q.ToListAsync(ct));
});

// Résumé d'assiduité pour un apprenant
reports.MapGet("/apprenant-attendance/{apprenantId:guid}", async (
    Guid apprenantId,
    AppDbContext db,
    [FromQuery(Name = "from")] DateTimeOffset fromDate,
    [FromQuery(Name = "to")]   DateTimeOffset toDate,
    CancellationToken ct) =>
{
    var stats = await (
        from pr in db.Presences
        join s in db.Seances on pr.SeanceId equals s.Id
        where pr.ApprenantId == apprenantId
              && s.Debut >= fromDate
              && s.Debut  < toDate
        group pr by 1 into g
        select new
        {
            Total   = g.Count(),
            Presents= g.Count(x => x.Statut == StatutPresence.Present || x.Justifie),
            Retards = g.Count(x => x.Statut == StatutPresence.Retard),
            Absents = g.Count(x => x.Statut == StatutPresence.Absent && !x.Justifie)
        }
    ).FirstOrDefaultAsync(ct);

    var total   = stats?.Total    ?? 0;
    var present = stats?.Presents ?? 0;
    var retards = stats?.Retards  ?? 0;
    var absents = stats?.Absents  ?? 0;

    return Results.Ok(new
    {
        ApprenantId = apprenantId,
        Total   = total,
        Presents= present,
        Retards = retards,
        Absents = absents,
        Taux    = total == 0 ? 0.0 : present * 100.0 / total
    });
});


// POST /api/auth/login  -> retourne { token, user { ... } }
app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db, CancellationToken ct) =>
{
    // 1) chercher l'utilisateur
    var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
    if (user is null)
        return Results.Unauthorized();

    // 2) vérifier mot de passe (BCrypt)
    if (!BCrypt.Net.BCrypt.Verify(req.Password, user.MotDePasseHash))
        return Results.Unauthorized();

    // 3) créer le JWT
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString())
    };

    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    // 4) réponse
    return Results.Ok(new
    {
        token = jwt,
        user = new { user.Id, user.Prenom, user.Nom, user.Email, Role = user.Role.ToString() }
    });
});

// GET /api/auth/me -> infos du porteur du token
app.MapGet("/api/auth/me", (ClaimsPrincipal me) =>
{
    if (me.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    return Results.Ok(new
    {
        Id = me.FindFirstValue(ClaimTypes.NameIdentifier),
        Name = me.Identity?.Name,
        Email = me.FindFirstValue(ClaimTypes.Email),
        Role = me.FindFirstValue(ClaimTypes.Role)
    });
}).RequireAuthorization();

app.Run();
record LoginRequest(string Email, string Password);
