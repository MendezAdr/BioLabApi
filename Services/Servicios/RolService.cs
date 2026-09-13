using BioLabApi.Models;
using BioLabApi.Models.DTOs;
using BioLabApi.Services.Interfaces;
using BioLabApi.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BioLabApi.Services.Servicios
{
    public class RolService : IRolService
    {
        private readonly AppDbContext _context;

        public RolService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolResponseDTO>> GetAllRolesAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            return roles.Select(MapToResponseDTO).ToList();
        }

        public async Task<RolResponseDTO> GetRolByIdAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return null;

            return MapToResponseDTO(rol);
        }

        public async Task<RolResponseDTO> CreateRolAsync(RolCreateDTO rolCreateDto)
        {
            // Validar si el nombre ya existe
            if (await _context.Roles.AnyAsync(r => r.RolName.ToLower() == rolCreateDto.Nombre.ToLower()))
            {
                throw new Exception("Ya existe un rol con ese nombre.");
            }

            var nuevoRol = new RolModel
            {
                RolName = rolCreateDto.Nombre,
                // TRADUCCIÓN: Convertimos la lista de permisos [1, 2, 16] en un solo Flag sumando sus bits
                Permisos = ConsolidarPermisos(rolCreateDto.Permisos)
            };

            _context.Roles.Add(nuevoRol);
            await _context.SaveChangesAsync();

            return MapToResponseDTO(nuevoRol);
        }

        public async Task<RolResponseDTO> UpdateRolAsync(RolUpdateDto rolUpdateDto)
        {
            var rolExistente = await _context.Roles.FindAsync(rolUpdateDto.Id);
            if (rolExistente == null) throw new KeyNotFoundException("Rol no encontrado.");

            // Validar que no se repita el nombre con OTRO rol
            if (await _context.Roles.AnyAsync(r => r.Id != rolUpdateDto.Id && r.RolName.ToLower() == rolUpdateDto.Nombre.ToLower()))
            {
                throw new Exception("Ya existe otro rol con ese nombre.");
            }

            rolExistente.RolName = rolUpdateDto.Nombre;
            // Actualizamos la sumatoria de bits
            rolExistente.Permisos = ConsolidarPermisos(rolUpdateDto.Permisos);

            _context.Roles.Update(rolExistente);
            await _context.SaveChangesAsync();

            return MapToResponseDTO(rolExistente);
        }

        public async Task<bool> DeleteRolAsync(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Usuarios) // Necesario para comprobar restricciones
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null) return false;

            // SEGURIDAD: No podemos borrar un rol si hay usuarios usándolo
            if (rol.Usuarios.Any())
            {
                throw new InvalidOperationException("No se puede eliminar un rol que está asignado a usuarios activos.");
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // MÉTODOS PRIVADOS DE MAPEO (Helpers)
        // ==========================================

        private RolResponseDTO MapToResponseDTO(RolModel rol)
        {
            return new RolResponseDTO
            {
                Id = rol.Id,
                RolName = rol.RolName, // Coincide con tu DTO
                // TRADUCCIÓN INVERSA: Del flag de DB (ej. 19) a la lista [1, 2, 16]
                Permisos = ExtraerListaPermisos(rol.Permisos)
            };
        }

        // Suma todos los permisos de la lista en un único Flag usando el operador bit a bit OR (|)
        private RolModel.PermisosSistema ConsolidarPermisos(List<RolModel.PermisosSistema> permisosLista)
        {
            if (permisosLista == null || !permisosLista.Any())
                return RolModel.PermisosSistema.Ninguno; // O un permiso 'Ninguno' si lo prefieres

            return permisosLista.Aggregate((RolModel.PermisosSistema)0, (current, permiso) => current | permiso);
        }

        // Desglosa el Flag guardado en la DB y crea una lista evaluando el ".HasFlag()"
        private List<RolModel.PermisosSistema> ExtraerListaPermisos(RolModel.PermisosSistema permisosFlag)
        {
            var lista = new List<RolModel.PermisosSistema>();

            // Si es 0 (Todos), lo agregamos directo
            if (permisosFlag == RolModel.PermisosSistema.Todos)
            {
                lista.Add(RolModel.PermisosSistema.Todos);
                return lista;
            }

            // Recorremos todos los valores del enum para ver cuáles están activos
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