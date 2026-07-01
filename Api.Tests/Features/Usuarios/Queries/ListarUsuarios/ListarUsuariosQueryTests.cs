using System.Net;
using System.Net.Http.Json;
using Api.Application.Features.Usuarios.Queries.ListarUsuarios;
using Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests.Features.Usuarios.Queries.ListarUsuarios;

public sealed class ListarUsuariosQueryTests : FunctionalTestBase
{
    [Test]
    public async Task ListarUsuarios_desde_mediatr_devuelve_resultado_paginado()
    {
        var result = await SendAsync(new ListarUsuariosQuery(1, 10));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Pagina.ShouldBe(1);
        result.Data.TamanoPagina.ShouldBe(10);
        result.Data.TotalRegistros.ShouldBe(25);
        result.Data.TotalPaginas.ShouldBe(3);
        result.Data.Items.Count.ShouldBe(10);
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_valida_paginacion()
    {
        var result = await SendAsync(new ListarUsuariosQuery(0, 101));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.Pagina));
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_valida_pagina_minima()
    {
        var result = await SendAsync(new ListarUsuariosQuery(0, 10));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.Pagina));
        result.Errors.ShouldNotContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_valida_pagina_maxima()
    {
        var result = await SendAsync(new ListarUsuariosQuery(100001, 10));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.Pagina));
        result.Errors.ShouldNotContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_valida_tamano_pagina_minimo()
    {
        var result = await SendAsync(new ListarUsuariosQuery(1, 0));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotContainKey(nameof(ListarUsuariosQuery.Pagina));
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_valida_tamano_pagina_maximo()
    {
        var result = await SendAsync(new ListarUsuariosQuery(1, 101));

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotContainKey(nameof(ListarUsuariosQuery.Pagina));
        result.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_devuelve_ultima_pagina_con_restante()
    {
        var result = await SendAsync(new ListarUsuariosQuery(3, 10));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Pagina.ShouldBe(3);
        result.Data.TamanoPagina.ShouldBe(10);
        result.Data.TotalRegistros.ShouldBe(25);
        result.Data.TotalPaginas.ShouldBe(3);
        result.Data.Items.Count.ShouldBe(5);
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_devuelve_lista_vacia_cuando_pagina_no_tiene_resultados()
    {
        var result = await SendAsync(new ListarUsuariosQuery(4, 10));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Pagina.ShouldBe(4);
        result.Data.TamanoPagina.ShouldBe(10);
        result.Data.TotalRegistros.ShouldBe(25);
        result.Data.TotalPaginas.ShouldBe(3);
        result.Data.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ListarUsuarios_desde_mediatr_respeta_tamano_pagina_maximo_permitido()
    {
        var result = await SendAsync(new ListarUsuariosQuery(1, 100));

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Pagina.ShouldBe(1);
        result.Data.TamanoPagina.ShouldBe(100);
        result.Data.TotalRegistros.ShouldBe(25);
        result.Data.TotalPaginas.ShouldBe(1);
        result.Data.Items.Count.ShouldBe(25);
    }

    [Test]
    public async Task ListarUsuarios_desde_mvc_testing_devuelve_ok()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/usuarios?pagina=2&tamanoPagina=5");
        var data = await response.Content.ReadFromJsonAsync<PaginatedResponse<UsuarioDto>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        data.ShouldNotBeNull();
        data.Pagina.ShouldBe(2);
        data.TamanoPagina.ShouldBe(5);
        data.TotalRegistros.ShouldBe(25);
        data.TotalPaginas.ShouldBe(5);
        data.Items.Count.ShouldBe(5);
    }

    [Test]
    public async Task ListarUsuarios_desde_mvc_testing_devuelve_bad_request_cuando_paginacion_es_invalida()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/usuarios?pagina=0&tamanoPagina=101");
        var data = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        data.ShouldNotBeNull();
        data.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.Pagina));
        data.Errors.ShouldContainKey(nameof(ListarUsuariosQuery.TamanoPagina));
    }
}
