using ConstructionSimulator.Models;

namespace ConstructionSimulator.Data
{
    public class InMemoryDataService
    {
        private static List<Project> _projects = new();
        private static List<Models.Task> _tasks = new();
        private static List<Crew> _crews = new();
        private static List<Material> _materials = new();
        private static List<Permit> _permits = new();
        private static List<SimulationLog> _simulationLogs = new();

        private static int _projectIdCounter = 1;
        private static int _taskIdCounter = 1;
        private static int _crewIdCounter = 1;
        private static int _materialIdCounter = 1;
        private static int _permitIdCounter = 1;
        private static int _logIdCounter = 1;

        public InMemoryDataService()
        {
            // Initialize with sample data if empty
            if (_projects.Count == 0)
            {
                InitializeSampleData();
            }
        }

        // Projects
        public List<Project> GetAllProjects() => _projects;
        public Project? GetProjectById(int id) => _projects.FirstOrDefault(p => p.ProjectID == id);

        public void AddProject(Project project)
        {
            project.ProjectID = _projectIdCounter++;
            _projects.Add(project);
        }

        public void UpdateProject(Project project)
        {
            var existing = _projects.FirstOrDefault(p => p.ProjectID == project.ProjectID);
            if (existing != null)
            {
                var index = _projects.IndexOf(existing);
                _projects[index] = project;
            }
        }

        public void DeleteProject(int id)
        {
            var project = _projects.FirstOrDefault(p => p.ProjectID == id);
            if (project != null)
            {
                _projects.Remove(project);
                // Also remove associated tasks
                _tasks.RemoveAll(t => t.ProjectID == id);
            }
        }

        // Tasks
        public List<Models.Task> GetAllTasks() => _tasks;
        public List<Models.Task> GetTasksByProjectId(int projectId) => _tasks.Where(t => t.ProjectID == projectId).ToList();
        public Models.Task? GetTaskById(int id) => _tasks.FirstOrDefault(t => t.TaskID == id);

        public void AddTask(Models.Task task)
        {
            task.TaskID = _taskIdCounter++;
            _tasks.Add(task);
            UpdateProjectCost(task.ProjectID);
        }

        public void UpdateTask(Models.Task task)
        {
            var existing = _tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
            if (existing != null)
            {
                var index = _tasks.IndexOf(existing);
                _tasks[index] = task;
                UpdateProjectCost(task.ProjectID);
            }
        }

        public void DeleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.TaskID == id);
            if (task != null)
            {
                var projectId = task.ProjectID;
                _tasks.Remove(task);
                UpdateProjectCost(projectId);
            }
        }

        // Crews
        public List<Crew> GetAllCrews() => _crews;
        public Crew? GetCrewById(int id) => _crews.FirstOrDefault(c => c.CrewID == id);

        public void AddCrew(Crew crew)
        {
            crew.CrewID = _crewIdCounter++;
            _crews.Add(crew);
        }

        public void UpdateCrew(Crew crew)
        {
            var existing = _crews.FirstOrDefault(c => c.CrewID == crew.CrewID);
            if (existing != null)
            {
                var index = _crews.IndexOf(existing);
                _crews[index] = crew;
            }
        }

        public void DeleteCrew(int id)
        {
            var crew = _crews.FirstOrDefault(c => c.CrewID == id);
            if (crew != null) _crews.Remove(crew);
        }

        // Materials
        public List<Material> GetAllMaterials() => _materials;
        public Material? GetMaterialById(int id) => _materials.FirstOrDefault(m => m.MaterialID == id);

        public void AddMaterial(Material material)
        {
            material.MaterialID = _materialIdCounter++;
            _materials.Add(material);
        }

        // Permits
        public List<Permit> GetAllPermits() => _permits;
        public Permit? GetPermitById(int id) => _permits.FirstOrDefault(p => p.PermitID == id);

        public void AddPermit(Permit permit)
        {
            permit.PermitID = _permitIdCounter++;
            _permits.Add(permit);
        }

        public void UpdatePermit(Permit permit)
        {
            var existing = _permits.FirstOrDefault(p => p.PermitID == permit.PermitID);
            if (existing != null)
            {
                var index = _permits.IndexOf(existing);
                _permits[index] = permit;
            }
        }

        // Simulation Logs
        public void AddSimulationLog(SimulationLog log)
        {
            log.LogID = _logIdCounter++;
            _simulationLogs.Add(log);
        }

        public List<SimulationLog> GetSimulationLogsByProjectId(int projectId)
        {
            return _simulationLogs.Where(l => l.ProjectID == projectId).OrderByDescending(l => l.Timestamp).ToList();
        }

        // Helper Methods
        private void UpdateProjectCost(int projectId)
        {
            var project = GetProjectById(projectId);
            if (project != null)
            {
                var projectTasks = GetTasksByProjectId(projectId);
                project.ActualCost = projectTasks.Sum(t => t.Cost);
            }
        }

        private void InitializeSampleData()
        {
            // Sample Crews
            _crews.Add(new Crew { CrewID = _crewIdCounter++, Name = "Alpha Team", SkillType = "General Construction", TeamSize = 5, HourlyRate = 150, IsAvailable = true });
            _crews.Add(new Crew { CrewID = _crewIdCounter++, Name = "Electrical Experts", SkillType = "Electrical", TeamSize = 3, HourlyRate = 200, IsAvailable = true });
            _crews.Add(new Crew { CrewID = _crewIdCounter++, Name = "Plumbing Pros", SkillType = "Plumbing", TeamSize = 4, HourlyRate = 180, IsAvailable = true });
            _crews.Add(new Crew { CrewID = _crewIdCounter++, Name = "Carpentry Masters", SkillType = "Carpentry", TeamSize = 4, HourlyRate = 160, IsAvailable = true });

            // Sample Permits
            _permits.Add(new Permit { PermitID = _permitIdCounter++, Type = "Building Permit", Status = "Approved", ApplicationDate = DateTime.Now.AddDays(-30), ApprovalDate = DateTime.Now.AddDays(-15), Fee = 5000, IssuingAuthority = "City Planning Department" });
            _permits.Add(new Permit { PermitID = _permitIdCounter++, Type = "Electrical Permit", Status = "Pending", ApplicationDate = DateTime.Now.AddDays(-10), Fee = 1500, IssuingAuthority = "Electrical Safety Board" });
            _permits.Add(new Permit { PermitID = _permitIdCounter++, Type = "Plumbing Permit", Status = "Approved", ApplicationDate = DateTime.Now.AddDays(-20), ApprovalDate = DateTime.Now.AddDays(-5), Fee = 1200, IssuingAuthority = "Water Authority" });

            // Sample Materials
            _materials.Add(new Material { MaterialID = _materialIdCounter++, Name = "Cement", Quantity = 100, Unit = "Bags", CostPerUnit = 15, LeadTimeDays = 3, Supplier = "BuildMart", InStock = true });
            _materials.Add(new Material { MaterialID = _materialIdCounter++, Name = "Steel Rebar", Quantity = 500, Unit = "Kg", CostPerUnit = 2.5m, LeadTimeDays = 7, Supplier = "SteelWorks Inc", InStock = true });
            _materials.Add(new Material { MaterialID = _materialIdCounter++, Name = "Electrical Wiring", Quantity = 1000, Unit = "Meters", CostPerUnit = 3.0m, LeadTimeDays = 5, Supplier = "ElectroSupply", InStock = false });

            // Sample Project 1 - Residential Building
            var project1 = new Project
            {
                ProjectID = _projectIdCounter++,
                Name = "Downtown Residential Complex",
                Description = "Construction of a 5-story residential building with 20 apartments",
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now.AddDays(150),
                Budget = 2500000,
                ActualCost = 0,
                Status = "In Progress"
            };
            _projects.Add(project1);

            // Tasks for Project 1
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project1.ProjectID, Name = "Site Preparation", Description = "Clear and level the construction site", StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-20), Duration = 10, Cost = 50000, CrewID = 1, Status = "Completed", Priority = "High" });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project1.ProjectID, Name = "Foundation Work", Description = "Pour foundation and basement", StartDate = DateTime.Now.AddDays(-19), EndDate = DateTime.Now.AddDays(10), Duration = 29, Cost = 350000, CrewID = 1, Status = "In Progress", Priority = "Critical", Dependencies = "1" });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project1.ProjectID, Name = "Structural Framework", Description = "Build main structural frame", StartDate = DateTime.Now.AddDays(11), EndDate = DateTime.Now.AddDays(50), Duration = 39, Cost = 600000, CrewID = 4, Status = "Pending", Priority = "Critical", Dependencies = "2" });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project1.ProjectID, Name = "Electrical Installation", Description = "Install electrical wiring and panels", StartDate = DateTime.Now.AddDays(51), EndDate = DateTime.Now.AddDays(80), Duration = 29, Cost = 250000, CrewID = 2, Status = "Pending", Priority = "High", Dependencies = "3", RequiresPermit = true, PermitID = 2 });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project1.ProjectID, Name = "Plumbing Installation", Description = "Install plumbing systems", StartDate = DateTime.Now.AddDays(51), EndDate = DateTime.Now.AddDays(75), Duration = 24, Cost = 200000, CrewID = 3, Status = "Pending", Priority = "High", Dependencies = "3", RequiresPermit = true, PermitID = 3 });

            // Sample Project 2 - Commercial Building
            var project2 = new Project
            {
                ProjectID = _projectIdCounter++,
                Name = "Tech Office Renovation",
                Description = "Renovation of existing office building for tech company",
                StartDate = DateTime.Now.AddDays(-15),
                EndDate = DateTime.Now.AddDays(90),
                Budget = 850000,
                ActualCost = 0,
                Status = "In Progress"
            };
            _projects.Add(project2);

            // Tasks for Project 2
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project2.ProjectID, Name = "Interior Demolition", Description = "Remove old fixtures and partitions", StartDate = DateTime.Now.AddDays(-15), EndDate = DateTime.Now.AddDays(-8), Duration = 7, Cost = 45000, CrewID = 1, Status = "Completed", Priority = "Medium" });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project2.ProjectID, Name = "New Layout Construction", Description = "Build new walls and partitions", StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now.AddDays(15), Duration = 22, Cost = 180000, CrewID = 4, Status = "In Progress", Priority = "High", Dependencies = "6" });
            _tasks.Add(new Models.Task { TaskID = _taskIdCounter++, ProjectID = project2.ProjectID, Name = "Modern Electrical Upgrade", Description = "Upgrade electrical systems for modern tech needs", StartDate = DateTime.Now.AddDays(16), EndDate = DateTime.Now.AddDays(35), Duration = 19, Cost = 120000, CrewID = 2, Status = "Pending", Priority = "Critical", Dependencies = "7" });

            // Sample Project 3 - Infrastructure
            var project3 = new Project
            {
                ProjectID = _projectIdCounter++,
                Name = "Bridge Repair Project",
                Description = "Structural repair and maintenance of highway bridge",
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddDays(120),
                Budget = 1200000,
                ActualCost = 0,
                Status = "Planning"
            };
            _projects.Add(project3);

            // Update project costs
            UpdateProjectCost(project1.ProjectID);
            UpdateProjectCost(project2.ProjectID);
        }
    }
}