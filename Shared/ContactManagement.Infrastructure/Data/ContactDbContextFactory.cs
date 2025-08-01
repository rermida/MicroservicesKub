using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // Adicionar esta diretiva 'using'
using System.IO; // Adicionar esta diretiva 'using'

namespace ContactManagement.Infrastructure.Data
{
    public class ContactDbContextFactory : IDesignTimeDbContextFactory<ContactDbContext>
    {
        public ContactDbContext CreateDbContext(string[] args)
        {
            // 1. Inicia o construtor de configuração, padrão do .NET
            var configuration = new ConfigurationBuilder()
                // 2. Define o caminho base como o diretório atual do projeto Infrastructure
                //    Isso é crucial para que ele encontre o arquivo appsettings.json
                .SetBasePath(Directory.GetCurrentDirectory())
                // 3. Adiciona o appsettings.json como fonte de configuração.
                //    O "optional: true" evita erro se o arquivo não existir.
                .AddJsonFile("appsettings.json", optional: true)
                // 4. Adiciona as variáveis de ambiente como fonte de configuração.
                //    ESTA É A PARTE ESSENCIAL PARA O KUBERNETES.
                //    Variáveis de ambiente sempre sobrescrevem as do appsettings.json.
                .AddEnvironmentVariables()
                // 5. Constrói o objeto de configuração final
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ContactDbContext>();

            // 6. Lê a connection string chamada "SqlServer" a partir das fontes configuradas
            var connectionString = configuration.GetConnectionString("SqlServer");

            // 7. Usa a connection string dinâmica para configurar o DbContext
            optionsBuilder.UseSqlServer(connectionString);

            return new ContactDbContext(optionsBuilder.Options);
        }
    }
}