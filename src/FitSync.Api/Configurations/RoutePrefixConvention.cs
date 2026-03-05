namespace FitSync.Api.Configurations;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class RoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel prefixRoute = new(new Microsoft.AspNetCore.Mvc.RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (ControllerModel controller in application.Controllers)
        {
            foreach (SelectorModel selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        this.prefixRoute,
                        selector.AttributeRouteModel
                    );
                else
                    selector.AttributeRouteModel = this.prefixRoute;
            }
        }
    }
}
