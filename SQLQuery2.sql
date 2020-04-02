 SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, e.Email, e.IsSupervisor, e.ComputerId, c.Id, 
                        c.PurchaseDate, c.DecomissionDate, c.Make, c.Model
                        FROM Employee e
                        LEFT JOIN Computer c
                        ON e.ComputerId = c.Id