using BioLabApi.Models;
using BioLabApi.Models.DTOs;
using BioLabApi.Services.Interfaces;
using BioLabApi.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioLabApi.Helpers; // Asumiendo que aquí residen OperationResult, ObjectOperationResult, etc.

namespace BioLabApi.Services.Servicios
{
    public class RolService : IRolService
    {
        private readonly AppDbContext _context;

        public RolService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ListOperationResult<RolResponseDTO>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                .AsNoTracking()
                .ToListAsync();
                var dtoList = roles.Select(MapToResponseDTO).ToList();
                return new ListOperationResult<RolResponseDTO>(true, "Roles obtenidos con éxito.", dtoList);
            }
            catch (Exception ex)
            {
                return new ListOperationResult<RolResponseDTO>(false, $"Error al obtener roles: {ex.Message}", null);
            }
        }

        public async Task<ObjectOperationResult> GetRolByIdAsync(int id)
        {
            try
            {
                var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
                if (rol == null) 
                    return new ObjectOperationResult(false, "Rol no encontrado.", null);

                return new ObjectOperationResult(true, "Rol obtenido.", MapToResponseDTO(rol));
            }
            catch (Exception ex)
            {
                return new ObjectOperationResult(false, $"Error al obtener el rol: {ex.Message}", null);
            }
        }

        public async Task<ObjectOperationResult> CreateRolAsync(RolCreateDTO rolCreateDto)
        {
            try
            {
                if (await _context.Roles.AnyAsync(r => r.RolName.ToLower() == rolCreateDto.Nombre.ToLower()))
                {
                    return new ObjectOperationResult(false, "Ya existe un rol con ese nombre.", null);
                }

                var nuevoRol = new RolModel
                {
                    RolName = rolCreateDto.Nombre,
                    Permisos = ConsolidarPermisos(rolCreateDto.Permisos)
                };

                _context.Roles.Add(nuevoRol);
                await _context.SaveChangesAsync();

                return new ObjectOperationResult(true, "Rol creado exitosamente.", MapToResponseDTO(nuevoRol));
            }
            catch (Exception ex)
            {
                return new ObjectOperationResult(false, $"Error al crear el rol: {ex.Message}", null);
            }
        }

        public async Task<ObjectOperationResult> UpdateRolAsync(RolUpdateDto rolUpdateDto)
        {
            try
            {
                var rolExistente = await _context.Roles.FindAsync(rolUpdateDto.Id);
                if (rolExistente == null) 
                    return new ObjectOperationResult(false, "Rol no encontrado.", null);

                if (await _context.Roles.AnyAsync(r => r.Id != rolUpdateDto.Id && r.RolName.ToLower() == rolUpdateDto.Nombre.ToLower()))
                {
                    return new ObjectOperationResult(false, "Ya existe otro rol con ese nombre.", null);
                }

                rolExistente.RolName = rolUpdateDto.Nombre;
                rolExistente.Permisos = ConsolidarPermisos(rolUpdateDto.Permisos);

                _context.Roles.Update(rolExistente);
                await _context.SaveChangesAsync();

                return new ObjectOperationResult(true, "Rol actualizado correctamente.", MapToResponseDTO(rolExistente));
            }
            catch (Exception ex)
            {
                return new ObjectOperationResult(false, $"Error al actualizar el rol: {ex.Message}", null);
            }
        }

        public async Task<OperationResult> DeleteRolAsync(int id)
        {
            try
            {
                var rol = await _context.Roles
                    .Include(r => r.Usuarios) 
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rol == null) 
                    return new OperationResult(false, "Rol no encontrado.");

                if (rol.Usuarios.Any())
                {
                    return new OperationResult(false, "No se puede eliminar un rol que está asignado a usuarios activos.");
                }

                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();
                return new OperationResult(true, "Rol eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error al eliminar el rol: {ex.Message}");
            }
        }

        // ==========================================
        // MÉTODOS PRIVADOS DE MAPEO (Helpers)
        // ==========================================

        private RolResponseDTO MapToResponseDTO(RolModel rol)
        {
            return new RolResponseDTO
            {
                Id = rol.Id,
                RolName = rol.RolName, 
                Permisos = ExtraerListaPermisos(rol.Permisos)
            };
        }

        private RolModel.PermisosSistema ConsolidarPermisos(List<RolModel.PermisosSistema> permisosLista)
        {
            if (permisosLista == null || !permisosLista.Any())
                return RolModel.PermisosSistema.Ninguno; 

            return permisosLista.Aggregate((RolModel.PermisosSistema)0, (current, permiso) => current | permiso);
        }

        private List<RolModel.PermisosSistema> ExtraerListaPermisos(RolModel.PermisosSistema permisosFlag)
        {
            var lista = new List<RolModel.PermisosSistema>();

            if (permisosFlag == RolModel.PermisosSistema.Todos)
            {
                lista.Add(RolModel.PermisosSistema.Todos);
                return lista;
            }

            foreach (RolModel.PermisosSistema permiso in Enum.GetValues(typeof(RolModel.PermisosSistema)))
            {
                if (permiso != RolModel.PermisosSistema.Todos && permisosFlag.HasFlag(permiso))
                {
                    lista.Add(permiso);
                }
            }

            return lista;
        }
    }
}