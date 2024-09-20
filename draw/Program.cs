using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=draw.db";

        // Inicializa o banco de dados e cria a tabela, se ainda não existir
        InicializarBancoDeDados(connectionString);

        Random random = new Random();
        int maxNumeros = 10; // Defina o número máximo a ser sorteado
        int novoNumero;

        Console.WriteLine("Sorteio de números! Pressione Enter para sortear um número.");
        Console.WriteLine("Digite 'sair' a qualquer momento para encerrar o programa.");

        while (true)
        {
            if (ObterTotalNumerosSorteados(connectionString) >= maxNumeros)
            {
                Console.WriteLine("Todos os números foram sorteados!");

                // Pergunta se o usuário quer resetar
                if (ConfirmarReset())
                {
                    LimparNumerosSorteados(connectionString);
                    Console.WriteLine("Todos os números foram reiniciados. O sorteio recomeça agora.");
                }
                else
                {
                    Console.WriteLine("Encerrando o programa. Até a próxima!");
                    break;
                }
            }
            else
            {
                Console.WriteLine("Pressione Enter para sortear um número:");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "sair")
                {
                    Console.WriteLine("Encerrando o programa. Até a próxima!");
                    break;
                }

                if (ObterTotalNumerosSorteados(connectionString) < maxNumeros)
                {
                    // Sorteia um número não repetido
                    do
                    {
                        novoNumero = random.Next(1, maxNumeros + 1);
                    } while (NumeroJaSorteado(connectionString, novoNumero));

                    // Insere o novo número sorteado no banco de dados
                    InserirNumeroSorteado(connectionString, novoNumero);

                    // Exibe o novo número sorteado
                    Console.WriteLine($"Número sorteado: {novoNumero}");

                    // Exibe todos os números já sorteados
                    var numerosSorteados = ObterNumerosSorteados(connectionString);
                    Console.WriteLine("Números sorteados até agora: ");
                    foreach (var numero in numerosSorteados)
                    {
                        Console.Write(numero + ", ");
                    }
                    Console.WriteLine(); // Quebra de linha para melhor visualização
                }
            }
        }
    }

    // Pergunta ao usuário se ele quer limpar os números e recomeçar
    static bool ConfirmarReset()
    {
        Console.WriteLine("Deseja limpar todos os números sorteados e recomeçar? (s/n)");
        string resposta = Console.ReadLine()?.Trim().ToLower();
        return resposta == "s";
    }

    static void InicializarBancoDeDados(string connectionString)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string tableCommand = @"CREATE TABLE IF NOT EXISTS NumerosSorteados (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    NumeroSorteado INT NOT NULL)";
            SqliteCommand createTable = new SqliteCommand(tableCommand, connection);
            createTable.ExecuteNonQuery();
        }
    }

    static bool NumeroJaSorteado(string connectionString, int numero)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectCommand = "SELECT COUNT(1) FROM NumerosSorteados WHERE NumeroSorteado = @Numero";
            SqliteCommand command = new SqliteCommand(selectCommand, connection);
            command.Parameters.AddWithValue("@Numero", numero);
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }

    static void InserirNumeroSorteado(string connectionString, int numero)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string insertCommand = "INSERT INTO NumerosSorteados (NumeroSorteado) VALUES (@Numero)";
            SqliteCommand command = new SqliteCommand(insertCommand, connection);
            command.Parameters.AddWithValue("@Numero", numero);
            command.ExecuteNonQuery();
        }
    }

    static List<int> ObterNumerosSorteados(string connectionString)
    {
        var numeros = new List<int>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectCommand = "SELECT NumeroSorteado FROM NumerosSorteados";
            SqliteCommand command = new SqliteCommand(selectCommand, connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    numeros.Add(reader.GetInt32(0));
                }
            }
        }
        return numeros;
    }

    static int ObterTotalNumerosSorteados(string connectionString)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string selectCommand = "SELECT COUNT(1) FROM NumerosSorteados";
            SqliteCommand command = new SqliteCommand(selectCommand, connection);
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count;
        }
    }

    static void LimparNumerosSorteados(string connectionString)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            string deleteCommand = "DELETE FROM NumerosSorteados";
            SqliteCommand command = new SqliteCommand(deleteCommand, connection);
            command.ExecuteNonQuery();
        }
    }
}
