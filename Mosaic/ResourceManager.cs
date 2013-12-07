using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mosaic
{
    /// <summary>
    /// Classe Singleton do Gerente de Memória
    /// </summary>
    class ResourceManager : IDisposable
    {
        #region Propriedades
        /// <summary>
        /// Instância
        /// </summary>
        public static ResourceManager Instance
        {
            get
            {
                return ResourceManagerCreator.CreatorInstance;
            }
        }
        #endregion

        #region Criador Singleton Multithread
        /// <summary>
        /// Classe de Criação do Singleton do ResourceManager para multithreads
        /// </summary>
        private sealed class ResourceManagerCreator
        {
            /// <summary>
            /// Instância da Classe Selada
            /// </summary>
            private static readonly ResourceManager _instance = new ResourceManager();

            /// <summary>
            /// Propriedade externada da instância
            /// </summary>
            public static ResourceManager CreatorInstance
            {
                get { return _instance; }
            }
        }
        #endregion

        #region Atributos
        /// <summary>
        /// Tabela Hash com os recursos
        /// </summary>
        protected Dictionary<String, IDisposable> mResourcesHash;

        /// <summary>
        /// Flag informativa para o sistema de gerência de memória sobre o estado do gerenciador de recursos.
        /// </summary>
        private bool Disposed = false;
        #endregion

        #region Construtores
        /// <summary>
        /// Construtor
        /// </summary>
        protected ResourceManager()
        {
            mResourcesHash = new Dictionary<String, IDisposable>();
        }
        #endregion

        #region Funções Membro
        /// <summary>
        /// Obtêm um recurso alocado
        /// </summary>
        /// <param name="Name">Nome ou Hash do Recurso</param>
        /// <returns>null se o recurso não estiver alocado e o recurso buscado caso contrário</returns>
        public IDisposable GetResource(String Name)
        {
            if (mResourcesHash.ContainsKey(Name))
            {
                return mResourcesHash[Name];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Adiciona um Recurso no Gerenciador
        /// </summary>
        /// <param name="Name">Nome ou Hash do Recurso</param>
        /// <param name="Res">Recurso</param>
        /// <returns>true se o recuso foi adicionado e false caso contrário</returns>
        /// <remarks>Se o recurso já existir, ele não é adicionado</remarks>
        public bool AddResource(String Name, IDisposable Res)
        {
            // Verifica se já existe
            if (mResourcesHash.ContainsKey(Name))
            {
                return false;
            }
            else
            {
                mResourcesHash.Add(Name, Res);

                // Muda estado de memória
                if (Disposed) Disposed = false;

                return true;
            }
        }

        /// <summary>
        /// Descarrega todos os recursos
        /// </summary>
        public void Dispose()
        {
            // Verificando se já foi descarregado
            if (!Disposed)
            {
                // Atualizando estado de descarga
                Disposed = true;

                // Descarregando
                foreach (IDisposable item in mResourcesHash.Values)
                {
                    item.Dispose();
                }

                // Limpando Tabela
                mResourcesHash.Clear();
            }
        }
        #endregion
    }
}
