using Mapping.Reflection.Models;
using System.Reflection;

UserEntity userEntity = new UserEntity
{
    //Id = 1,
    //FirstName = "Tech",
    //LastName = "Buddy",
    EmailAddress = "techbuddy@gmail.com",
    //Addresses = new List<Address> { new Address() { Line1 = "Istanbul" } }
};



var userDto = ObjectMapper.MapGenericWithCustomMapping<UserEntity, UserDto>(userEntity);



Console.ReadLine();


class ObjectMapper
{
    public static UserDto Map(UserEntity userEntity)
    {
        var userDto = new UserDto();
        var entityProperties = typeof(UserEntity).GetProperties();
        var userDtoProperties = typeof(UserDto).GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var dtoProperty = userDtoProperties
                .FirstOrDefault(p => p.Name == entityProperty.Name);

            if (dtoProperty is not null)
            {
                var value = entityProperty.GetValue(userEntity);
                dtoProperty.SetValue(userDto, value);
            }
        }

        return userDto;
    }

    public static TDto MapGeneric<TEntity, TDto>(TEntity entity)
    {
        var dto = Activator.CreateInstance<TDto>();
        var dtoProperties = typeof(TDto).GetProperties();
        var entityProperties = typeof(TEntity).GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == entityProperty.Name);

            if (dtoProperty != null)
            {
                dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
            }
        }

        return dto;
    }

    public static TDto MapGenericNestedProperties<TEntity, TDto>(TEntity entity)
    {
        var dto = Activator.CreateInstance<TDto>();
        var dtoProperties = typeof(TDto).GetProperties();
        var entityProperties = typeof(TEntity).GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == entityProperty.Name);

            if (dtoProperty != null)
            {
                if (dtoProperty.PropertyType.IsPrimitive || dtoProperty.PropertyType == typeof(string))
                {
                    dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
                }
                else
                {
                    var nestedEntity = entityProperty.GetValue(entity);
                    var nestedDto = Activator.CreateInstance(dtoProperty.PropertyType);

                    var nestedDtoProperties = dtoProperty.PropertyType.GetProperties();

                    foreach (var nestedEntityProperty in nestedEntity.GetType().GetProperties())
                    {
                        var nestedDtoProperty = nestedDtoProperties.FirstOrDefault(p => p.Name == nestedEntityProperty.Name);

                        if (nestedDtoProperty != null)
                        {
                            nestedDtoProperty.SetValue(nestedDto, nestedEntityProperty.GetValue(nestedEntity));
                        }
                    }

                    dtoProperty.SetValue(dto, nestedDto);
                }
            }
        }

        return dto;
    }

    public static object MapGenericNestedPropertiesRecursive<TEntity>(TEntity entity, Type dtoType)
    {
        var dto = Activator.CreateInstance(dtoType);
        var dtoProperties = dtoType.GetProperties();
        var entityProperties = entity.GetType().GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == entityProperty.Name);

            if (dtoProperty == null)
                continue;

            if (dtoProperty.PropertyType.IsPrimitive || dtoProperty.PropertyType == typeof(string))
            {
                dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
            }
            else if (dtoProperty.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                // TODO ->
                dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
            }
            else
            {
                var nestedEntity = entityProperty.GetValue(entity);
                //var nestedDto = Activator.CreateInstance(dtoProperty.PropertyType);

                var nestedDto = MapGenericNestedPropertiesRecursive(nestedEntity, dtoProperty.PropertyType);
                dtoProperty.SetValue(dto, nestedDto);
            }
        }

        return dto;
    }

    public static TDto MapGenericWithCustomMapping<TEntity, TDto>(TEntity entity)
    {
        var dto = Activator.CreateInstance<TDto>();
        var dtoProperties = typeof(TDto).GetProperties();
        var entityProperties = typeof(TEntity).GetProperties();

        foreach (var entityProperty in entityProperties)
        {
            var customMappingAttribute = entityProperty.GetCustomAttribute<CustomAttribute>();
            var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == customMappingAttribute?.PropertyName
                                                        || p.Name == entityProperty.Name);

            if (dtoProperty != null)
            {
                dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
            }
        }

        return dto;
    }
}


class CustomAttribute: Attribute
{
    public string PropertyName { get; private set; }

    public CustomAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}