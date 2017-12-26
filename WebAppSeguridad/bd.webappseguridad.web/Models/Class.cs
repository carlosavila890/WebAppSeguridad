﻿using bd.webappseguridad.entidades.Utils;
using bd.webappseguridad.servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace bd.webappseguridad.web.Models
{

    public class RolesHandler : AuthorizationHandler<RolesRequirement>
    {
        public readonly IApiServicio apiServicio;

        public RolesHandler(IApiServicio apiServicio)
        {
            this.apiServicio = apiServicio;
        }
        /// <summary>
        /// Método que válida si el usuario actual tiene permiso para realizar la acción solicitada en el contecto 
        /// El usuario debe tener los permisos en la base de datos
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns>Si la acción solicitada es o no satisfactoria</returns>
        protected  override  Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
        {
            try
            {
                //Obtención del Context
                var recurso = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

                var httpContext = recurso.HttpContext;
                var request = httpContext.Request;
                var claim = context.User.Identities.Where(x => x.NameClaimType == ClaimTypes.Name).FirstOrDefault();
                var token = claim.Claims.Where(c => c.Type == ClaimTypes.SerialNumber).FirstOrDefault().Value;
                var NombreUsuario = claim.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault().Value;

                var permiso = new PermisoUsuario
                {
                    Contexto= request.Path,
                    Token=token,
                    Usuario=NombreUsuario,
                };

               var respuesta=  apiServicio.ObtenerElementoAsync1<Response>(permiso, new Uri(WebApp.BaseAddress), "api/Adscpassws/TienePermiso");

                //respuesta.Result.IsSuccess = true;
                if (respuesta.Result.IsSuccess)
                {
                    context.Succeed(requirement);

                }
                else
                {
                    context.Fail();
                }
                
            }
            catch (Exception ex)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            
            return Task.FromResult(0);
        }
    }
}
