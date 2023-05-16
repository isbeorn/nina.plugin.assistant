﻿using Assistant.NINAPlugin.Database.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace NINA.Plugin.Assistant.Test.Database.Schema {

    [TestFixture]
    public class ProfilePreferenceTest {

        [Test]
        public void TestProfilePreference() {
            ProfilePreference sut = new ProfilePreference("pid");
            sut.ProfileId.Should().Be("pid");
            sut.ParkOnWait.Should().BeFalse();
            sut.ExposureThrottle.Should().BeApproximately(125, 0.001);
            sut.EnableGradeRMS.Should().BeTrue();
            sut.EnableGradeStars.Should().BeTrue();
            sut.EnableGradeHFR.Should().BeTrue();
            sut.AcceptImprovement.Should().BeTrue();
            sut.MaxGradingSampleSize.Should().Be(10);
            sut.RMSPixelThreshold.Should().Be(8);
            sut.DetectedStarsSigmaFactor.Should().BeApproximately(4, 0.001);
            sut.HFRSigmaFactor.Should().BeApproximately(4, 0.001);
        }
    }
}
