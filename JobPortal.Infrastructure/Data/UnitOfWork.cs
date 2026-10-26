using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JobPortal.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // 2601 і 2627 — коди MS SQL для порушення унікального індексу.
            // Без цього гонка дала б клієнту 500 замість зрозумілого 409.
            throw new ConflictException("Такий запис уже існує");
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is SqlException sql
           && (sql.Number == 2601 || sql.Number == 2627);
}