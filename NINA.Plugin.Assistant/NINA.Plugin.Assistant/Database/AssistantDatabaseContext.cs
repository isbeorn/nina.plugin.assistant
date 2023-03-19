﻿using Assistant.NINAPlugin.Database.Schema;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Assistant.NINAPlugin.Database {

    public class AssistantDatabaseContext : DbContext {

        public DbSet<Project> ProjectSet { get; set; }
        public DbSet<RuleWeight> RuleWeightSet { get; set; }
        public DbSet<Target> TargetSet { get; set; }
        public DbSet<ExposurePlan> ExposurePlanSet { get; set; }
        public DbSet<ExposureTemplate> ExposureTemplateSet { get; set; }
        public DbSet<AcquiredImage> AcquiredImageSet { get; set; }
        public DbSet<ImageData> ImageDataSet { get; set; }

        public AssistantDatabaseContext(string connectionString) : base(new SQLiteConnection() { ConnectionString = connectionString }, true) {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            Logger.Debug("Scheduler database: OnModelCreating");

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new ProjectConfiguration());
            modelBuilder.Configurations.Add(new TargetConfiguration());
            modelBuilder.Configurations.Add(new ExposureTemplateConfiguration());
            modelBuilder.Configurations.Add(new AcquiredImageConfiguration());

            var sqi = new CreateOrMigrateDatabaseInitializer<AssistantDatabaseContext>();
            System.Data.Entity.Database.SetInitializer(sqi);
        }

        public List<Project> GetAllProjects(string profileId) {
            return ProjectSet
                .Include("targets.exposureplans.exposuretemplate")
                .Include("ruleweights")
                .Where(p => p.ProfileId
                .Equals(profileId))
                .ToList();
        }

        public List<Project> GetActiveProjects(string profileId, DateTime atTime) {
            long secs = DateTimeToUnixSeconds(atTime);
            var projects = ProjectSet
                .Include("targets.exposureplans.exposuretemplate")
                .Include("ruleweights")
                .Where(p =>
                p.ProfileId.Equals(profileId) &&
                p.state_col == (int)ProjectState.Active &&
                p.startDate <= secs && secs <= p.endDate);
            return projects.ToList();
        }

        public bool HasActiveTargets(string profileId, DateTime atTime) {
            long secs = DateTimeToUnixSeconds(atTime);
            List<Project> projects = ProjectSet
                .AsNoTracking()
                .Include("targets")
                .Where(p =>
                p.ProfileId.Equals(profileId) &&
                p.state_col == (int)ProjectState.Active &&
                p.startDate <= secs && secs <= p.endDate).ToList();

            foreach (Project project in projects) {
                foreach (Target target in project.Targets) {
                    if (target.Enabled) { return true; }
                }
            }

            return false;
        }

        public List<ExposureTemplate> GetExposureTemplates(string profileId) {
            return ExposureTemplateSet.Where(p => p.profileId == profileId).ToList();
        }

        public Project GetProject(int projectId) {
            return ProjectSet
                .Include("targets.exposureplans.exposuretemplate")
                .Include("ruleweights")
                .Where(p => p.Id == projectId)
                .FirstOrDefault();
        }

        public Target GetTarget(int projectId, int targetId) {
            return TargetSet
                .Include("exposureplans.exposuretemplate")
                .Where(t => t.Project.Id == projectId && t.Id == targetId)
                .FirstOrDefault();
        }

        public Target GetTargetByProject(int projectId, int targetId) {
            Project project = GetProject(projectId);
            return project.Targets.Where(t => t.Id == targetId).FirstOrDefault();
        }

        public ExposurePlan GetExposurePlan(int id) {
            return ExposurePlanSet
                .Include("exposuretemplate")
                .Where(p => p.Id == id)
                .FirstOrDefault();
        }

        public ExposureTemplate GetExposureTemplate(int id) {
            return ExposureTemplateSet.Where(e => e.Id == id).FirstOrDefault();
        }

        public List<AcquiredImage> GetAcquiredImages(int targetId, string filterName) {
            var images = AcquiredImageSet.Where(p =>
                p.TargetId == targetId &&
                p.FilterName == filterName)
              .OrderByDescending(p => p.acquiredDate);
            return images.ToList();
        }

        public ImageData GetImageData(int acquiredImageId) {
            return ImageDataSet.Where(d => d.AcquiredImageId == acquiredImageId).FirstOrDefault();
        }

        public ImageData GetImageData(int acquiredImageId, string tag) {
            return ImageDataSet.Where(d =>
                d.AcquiredImageId == acquiredImageId &&
                d.tag == tag).FirstOrDefault();
        }

        public Project AddNewProject(Project project) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    ProjectSet.Add(project);
                    SaveChanges();
                    transaction.Commit();
                    return GetProject(project.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error adding new project: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public Project SaveProject(Project project) {
            Logger.Debug($"Scheduler: saving Project Id={project.Id} Name={project.Name}");
            using (var transaction = Database.BeginTransaction()) {
                try {
                    ProjectSet.AddOrUpdate(project);
                    project.RuleWeights.ForEach(item => RuleWeightSet.AddOrUpdate(item));
                    SaveChanges();
                    transaction.Commit();
                    return GetProject(project.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error persisting Project: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public Project PasteProject(string profileId, Project source) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    Project project = source.GetPasteCopy(profileId);
                    ProjectSet.Add(project);
                    SaveChanges();
                    transaction.Commit();
                    return GetProject(project.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error pasting project: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public bool DeleteProject(Project project) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    project = GetProject(project.Id);
                    ProjectSet.Remove(project);
                    SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error deleting project: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return false;
                }
            }
        }

        public Target AddNewTarget(Project project, Target target) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    project = GetProject(project.Id);
                    project.Targets.Add(target);
                    SaveChanges();
                    transaction.Commit();
                    return GetTarget(target.Project.Id, target.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error adding new target: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public Target SaveTarget(Target target) {
            Logger.Debug($"Scheduler: saving Target Id={target.Id} Name={target.Name}");
            using (var transaction = Database.BeginTransaction()) {
                try {
                    TargetSet.AddOrUpdate(target);
                    target.ExposurePlans.ForEach(plan => {
                        plan.ExposureTemplate = null; // clear this (ExposureTemplateId handles the relation)
                        ExposurePlanSet.AddOrUpdate(plan);
                        plan.ExposureTemplate = GetExposureTemplate(plan.ExposureTemplateId); // add back for UI
                    });

                    SaveChanges();
                    transaction.Commit();
                    return GetTarget(target.Project.Id, target.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error persisting Target: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public Target PasteTarget(Project project, Target source) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    Target target = source.GetPasteCopy(project.ProfileId);
                    project = GetProject(project.Id);
                    project.Targets.Add(target);
                    SaveChanges();
                    transaction.Commit();
                    return GetTarget(project.Id, target.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error pasting target: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public bool DeleteTarget(Target target) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    target = GetTarget(target.ProjectId, target.Id);
                    TargetSet.Remove(target);
                    SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error deleting target: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return false;
                }
            }
        }

        public bool SaveExposurePlan(ExposurePlan exposurePlan) {
            Logger.Debug($"Scheduler: saving Exposure Plan Id={exposurePlan.Id}");
            using (var transaction = Database.BeginTransaction()) {
                try {
                    ExposurePlanSet.AddOrUpdate(exposurePlan);
                    SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error persisting Exposure Plan: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return false;
                }
            }
        }

        public Target DeleteExposurePlan(Target target, ExposurePlan exposurePlan) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    exposurePlan = GetExposurePlan(exposurePlan.Id);
                    ExposurePlanSet.Remove(exposurePlan);
                    SaveChanges();
                    transaction.Commit();
                    return GetTargetByProject(target.ProjectId, target.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error deleting Filter Plan: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public ExposureTemplate SaveExposureTemplate(ExposureTemplate exposureTemplate) {
            Logger.Debug($"Scheduler: saving Exposure Template Id={exposureTemplate.Id} Name={exposureTemplate.Name}");
            using (var transaction = Database.BeginTransaction()) {
                try {
                    ExposureTemplateSet.AddOrUpdate(exposureTemplate);
                    SaveChanges();
                    transaction.Commit();
                    return GetExposureTemplate(exposureTemplate.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error persisting Exposure Template: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public ExposureTemplate PasteExposureTemplate(string profileId, ExposureTemplate source) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    ExposureTemplate exposureTemplate = source.GetPasteCopy(profileId);
                    ExposureTemplateSet.Add(exposureTemplate);
                    SaveChanges();
                    transaction.Commit();
                    return GetExposureTemplate(exposureTemplate.Id);
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error pasting exposure template: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return null;
                }
            }
        }

        public bool DeleteExposureTemplate(ExposureTemplate exposureTemplate) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    exposureTemplate = GetExposureTemplate(exposureTemplate.Id);
                    ExposureTemplateSet.Remove(exposureTemplate);
                    SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error deleting exposure template: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                    return false;
                }
            }
        }

        public void AddExposureTemplates(List<ExposureTemplate> exposureTemplates) {
            using (var transaction = Database.BeginTransaction()) {
                try {
                    exposureTemplates.ForEach(exposureTemplate => {
                        ExposureTemplateSet.AddOrUpdate(exposureTemplate);
                    });

                    SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e) {
                    Logger.Error($"Scheduler: error persisting Filter Preferences: {e.Message} {e.StackTrace}");
                    RollbackTransaction(transaction);
                }
            }
        }

        public static long DateTimeToUnixSeconds(DateTime? dateTime) {
            return dateTime == null ? 0 : CoreUtil.DateTimeToUnixTimeStamp((DateTime)dateTime);
        }

        public static DateTime UnixSecondsToDateTime(long? seconds) {
            return CoreUtil.UnixTimeStampToDateTime(seconds == null ? 0 : seconds.Value);
        }

        private static void RollbackTransaction(DbContextTransaction transaction) {
            try {
                Logger.Warning("Scheduler: rolling back database changes");
                transaction.Rollback();
            }
            catch (Exception e) {
                Logger.Error($"Scheduler: error executing transaction rollback: {e.Message} {e.StackTrace}");
            }
        }

        /* DATABASE MIGRATION NOTES
         * - See NINA NINADbContext.Migrate()
         * - Basically, this uses 'PRAGMA user_version' to get the version of the database.  Defaults to 0.
         * - It delivers new updates in files named N.sql in C:\Program Files\N.I.N.A. - Nighttime Imaging 'N' Astronomy\Database\Migration\
         * - Each of those will do whatever updates are needed and then execute 'PRAGMA user_version = N;' at the end.
         * - Should be able to write unit tests for the migration files.
         */

        private class CreateOrMigrateDatabaseInitializer<TContext> : CreateDatabaseIfNotExists<TContext>,
                IDatabaseInitializer<TContext> where TContext : AssistantDatabaseContext {

            void IDatabaseInitializer<TContext>.InitializeDatabase(TContext context) {

                if (!DatabaseExists(context)) {
                    Logger.Debug("Assistant database: creating database schema");
                    using (var transaction = context.Database.BeginTransaction()) {
                        try {
                            context.Database.ExecuteSqlCommand(GetInitialSQL());
                            transaction.Commit();
                        }
                        catch (Exception e) {
                            Logger.Error($"Scheduler: error creating or initializing database: {e.Message} {e.StackTrace}");
                            RollbackTransaction(transaction);
                        }
                    }
                }
            }

            private bool DatabaseExists(TContext context) {
                int numTables = context.Database.SqlQuery<int>("SELECT COUNT(*) FROM sqlite_master AS TABLES WHERE TYPE = 'table'").First();
                return numTables > 0;
            }

            private string GetInitialSQL() {
                try {
                    ResourceManager rm = new ResourceManager("Assistant.NINAPlugin.Database.Initial.SQL", Assembly.GetExecutingAssembly());
                    return (string)rm.GetObject("initial_schema");
                }
                catch (Exception ex) {
                    Logger.Error($"failed to load Scheduler database initial SQL: {ex.Message}");
                    throw ex;
                }
            }

        }

    }
}
