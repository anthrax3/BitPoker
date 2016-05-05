﻿using System;
using System.Collections.Generic;
using BitPoker.Models.Contracts;

namespace BitPoker.Repository
{
    public class MockTableRepo : ITableRepository
    {
        public Table Find(Guid id)
        {
            //Return a fake contract
            //{D6D9890D-0CA2-4B5D-AE98-FA4D45EB4363}
            return new Models.Contracts.Table(2, 10) { Id = new Guid("D6D9890D-0CA2-4B5D-AE98-FA4D45EB4363") };
        }

        public IEnumerable<Table> All()
        {
            throw new NotImplementedException();
        }

        public void Add(Table item)
        {
        }

        public void Dispose()
        {
        }
    }
}
