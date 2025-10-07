namespace SewerDeformationSoftware.Logics.Converters;

public class ListMVC : IMultiValueConverter
{
    public Object[] ConvertBack(Object Value, Type[] TargetTypes, Object Parameter, CultureInfo Culture)

                                                                         => (Object[])Binding.DoNothing;

    public Object Convert(Object[] Values, Type TargetType, Object Parameter, CultureInfo Culture)

                                                                      => new List<Object>(Values);
}